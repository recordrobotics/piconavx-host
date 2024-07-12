using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace piconavx
{
    public class Server : IDisposable
    {
        public List<Client> Clients { get; } = new List<Client>();
        public int ClientTimeout = 1000;
        // Longer timeout for high bandwidth clients because they send less frequent updates to fit more data through
        public int ClientHighBandwidthTimeout = 10000;

        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

        private TcpListener listener;
        private Task? acceptTask = null;
        private CancellationTokenSource cancelSource;

        public Server(int port)
        {
            cancelSource = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
        }

        private void FireClientConnected(object? sender, ClientConnectedEventArgs eventArgs)
        {
            ClientConnected?.Invoke(this, eventArgs);
        }

        private void FireClientDisconnected(object? sender, ClientDisconnectedEventArgs eventArgs)
        {
            ClientDisconnected?.Invoke(this, eventArgs);
        }

        private async Task<Exception?> ReadClient(Client client, CancellationToken cancellationToken)
        {
            Exception? exc = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using CancellationTokenSource source = new CancellationTokenSource();
                    source.CancelAfter(client.HighBandwidthMode ? ClientHighBandwidthTimeout : ClientTimeout);
                    string? line = await client.Reader.ReadLineAsync(source.Token);
                    if (line == null)
                    {
                        Debug.WriteLine("Error: client reader got null");
                        break;
                    }

                    DataType dataType = Protocol.ParseDataType(line);

                    switch (dataType)
                    {
                        case DataType.YPRUpdate:
                            {
                                YPRUpdate yprUpdate = Protocol.ParseYPRUpdate(line);
                                client.FireUpdateReceieved(this, new ClientUpdateReceivedEventArgs(client, yprUpdate));
                            }
                            break;
                        case DataType.RawUpdate:
                            {
                                RawUpdate rawUpdate = Protocol.ParseRawUpdate(line);
                                client.FireUpdateReceieved(this, new ClientUpdateReceivedEventArgs(client, rawUpdate));
                            }
                            break;
                        case DataType.AHRSPosUpdate:
                            {
                                AHRSPosUpdate ahrsPosUpdate = Protocol.ParseAHRSPosUpdate(line);
                                client.FireUpdateReceieved(this, new ClientUpdateReceivedEventArgs(client, ahrsPosUpdate));
                            }
                            break;
                        case DataType.AHRSUpdate:
                            {
                                AHRSUpdate ahrsUpdate = Protocol.ParseAHRSUpdate(line);
                                client.FireUpdateReceieved(this, new ClientUpdateReceivedEventArgs(client, ahrsUpdate));
                            }
                            break;
                        case DataType.HealthUpdate:
                            {
                                HealthUpdate healthUpdate = Protocol.ParseHealthUpdate(line);
                                client.Health = healthUpdate;
                                client.FireRequestReturned(this, new ClientRequestReturnedEventArgs(client, DataType.HealthUpdate));
                            }
                            break;
                        case DataType.BoardStateUpdate:
                            {
                                BoardStateUpdate boardStateUpdate = Protocol.ParseBoardStateUpdate(line);
                                client.BoardState = boardStateUpdate;
                                client.FireRequestReturned(this, new ClientRequestReturnedEventArgs(client, DataType.BoardStateUpdate));
                            }
                            break;
                        case DataType.BoardIdUpdate:
                            {
                                BoardIdUpdate boardIdUpdate = Protocol.ParseBoardIdUpdate(line);
                                client.BoardId = boardIdUpdate;
                                client.FireRequestReturned(this, new ClientRequestReturnedEventArgs(client, DataType.BoardIdUpdate));
                            }
                            break;
                        case DataType.FeedUpdate:
                            {
                                FeedChunk[] feedUpdate = await Protocol.ParseFeedUpdate(line, client.Reader.BaseStream, source.Token);
                                client.FireUpdateReceieved(this, new ClientUpdateReceivedEventArgs(client, feedUpdate));
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    exc = e;
                    break;
                }
            }

            return exc;
        }

        private async Task<Exception?> WriteClient(Client client, CancellationToken cancellationToken)
        {
            Exception? exc = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (client.CommandQueue.TryDequeue(out ClientCommand? command) && command != null)
                    {
                        switch (command.CommandType)
                        {
                            case HostCommandType.SetDataType:
                                HostSetDataType value = (HostSetDataType)command.Data!;
                                client.HighBandwidthMode = value == HostSetDataType.Feed; // enable high bandwidth mode for feed updates
                                await client.Writer.WriteLineAsync(Protocol.SerializeSetDataTypeCommand(value));
                                await client.Writer.FlushAsync(cancellationToken);
                                break;
                            case HostCommandType.RequestHealth:
                                await client.Writer.WriteLineAsync(Protocol.SerializeRequestHealthCommand());
                                await client.Writer.FlushAsync(cancellationToken);
                                break;
                            case HostCommandType.RequestBoardState:
                                await client.Writer.WriteLineAsync(Protocol.SerializeRequestBoardStateCommand());
                                await client.Writer.FlushAsync(cancellationToken);
                                break;
                            case HostCommandType.RequestBoardId:
                                await client.Writer.WriteLineAsync(Protocol.SerializeRequestBoardIdCommand());
                                await client.Writer.FlushAsync(cancellationToken);
                                break;
                            case HostCommandType.ZeroYaw:
                                await client.Writer.WriteLineAsync(Protocol.SerializeZeroYawCommand());
                                await client.Writer.FlushAsync(cancellationToken);
                                break;
                            case HostCommandType.ZeroDisplacement:
                                await client.Writer.WriteLineAsync(Protocol.SerializeZeroDisplacementCommand());
                                await client.Writer.FlushAsync(cancellationToken);
                                break;
                            case HostCommandType.SetFeedOverflow:
                                await client.Writer.WriteLineAsync(Protocol.SerializeSetFeedOverflowCommand((HostSetFeedOverflowType)command.Data!));
                                await client.Writer.FlushAsync(cancellationToken);
                                break;
                        }
                    }
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    exc = e;
                    break;
                }
            }

            return exc;
        }

        private async Task HandleClient(TcpClient client)
        {
            using StreamReader reader = new StreamReader(client.GetStream());
            using StreamWriter writer = new StreamWriter(client.GetStream());
            using CancellationTokenSource source = new CancellationTokenSource();

            Client dataClient = new Client(null, client, reader, writer);

            if (await Protocol.IdentifyClient(dataClient, source.Token))
            {
                dataClient.Connected = true;
                Clients.Add(dataClient);
                FireClientConnected(this, new ClientConnectedEventArgs(dataClient));

                Task<Exception?> read = ReadClient(dataClient, source.Token);
                Task<Exception?> write = WriteClient(dataClient, source.Token);

                while (!cancelSource.IsCancellationRequested && client.Connected && !read.GetAwaiter().IsCompleted && !write.GetAwaiter().IsCompleted)
                {
                    await Task.Delay(20);
                }

                source.Cancel();
                await Task.WhenAll(read, write);
                dataClient.Connected = false;
                FireClientDisconnected(this, new ClientDisconnectedEventArgs(dataClient, read.Result, write.Result));
                Clients.Remove(dataClient);
            }
            else
            {
                client.Close();
            }
        }

        private async Task AcceptTask()
        {
            List<Task> tasks = new List<Task>();
            while (!cancelSource.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(cancelSource.Token);
                    Task? task = null;
                    task = HandleClient(client);
                    tasks.Add(task);
                    tasks.RemoveAll((t) => t.IsCompleted);
                }
                catch
                {
                    break;
                }
            }
            Task.WaitAll(tasks.ToArray());
        }

        public async Task Start()
        {
            listener.Start();
            acceptTask = Task.Run(AcceptTask, cancelSource.Token);
            await acceptTask;
        }

        public void Stop()
        {
            cancelSource.Cancel();
            acceptTask?.Wait();
            listener.Stop();
        }

        public void Dispose()
        {
            cancelSource.Dispose();
            listener.Dispose();
        }
    }

    public class ClientConnectedEventArgs : EventArgs
    {
        public Client Client { get; }

        public ClientConnectedEventArgs(Client client)
        {
            Client = client;
        }
    }

    public class ClientDisconnectedEventArgs : EventArgs
    {
        public Client Client { get; }
        public Exception? ReadException { get; }
        public Exception? WriteException { get; }

        public ClientDisconnectedEventArgs(Client client, Exception? readException, Exception? writeException)
        {
            Client = client;
            ReadException = readException;
            WriteException = writeException;
        }
    }
}
