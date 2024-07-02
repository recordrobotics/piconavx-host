using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace piconavx
{
    public class Client
    {
        public bool Connected { get; set; } = false;
        public string? Id { get; set; }
        public TcpClient Tcp { get; }
        public StreamReader Reader { get; }
        public StreamWriter Writer { get; }
        public ConcurrentQueue<ClientCommand> CommandQueue { get; }

        public HealthUpdate Health { get; set; }
        public BoardIdUpdate BoardId { get; set; }
        public BoardStateUpdate BoardState { get; set; }

        public event EventHandler<ClientRequestReturnedEventArgs>? RequestReturned;
        public event EventHandler<ClientUpdateReceivedEventArgs>? UpdateReceieved;

        public Client(string? id, TcpClient tcp, StreamReader reader, StreamWriter writer)
        {
            Id = id;
            Tcp = tcp;
            Reader = reader;
            Writer = writer;
            CommandQueue = new ConcurrentQueue<ClientCommand>();
        }

        public void SetDataType(HostSetDataType dataType)
        {
            CommandQueue.Enqueue(new ClientCommand(HostCommandType.SetDataType, dataType));
        }

        public void RequestHealth()
        {
            CommandQueue.Enqueue(new ClientCommand(HostCommandType.RequestHealth));
        }

        public void RequestBoardState()
        {
            CommandQueue.Enqueue(new ClientCommand(HostCommandType.RequestBoardState));
        }

        public void RequestBoardId()
        {
            CommandQueue.Enqueue(new ClientCommand(HostCommandType.RequestBoardId));
        }

        public void ZeroYaw()
        {
            CommandQueue.Enqueue(new ClientCommand(HostCommandType.ZeroYaw));
        }

        public void ZeroDisplacement()
        {
            CommandQueue.Enqueue(new ClientCommand(HostCommandType.ZeroDisplacement));
        }

        public async Task<HealthUpdate> GetHealth()
        {
            RequestHealth();

            using CancellationTokenSource source = new CancellationTokenSource();
            EventHandler<ClientRequestReturnedEventArgs>? handler = null;
            handler = (s,e) =>
            {
                if (handler != null && e.DataType == DataType.HealthUpdate)
                {
                    source.Cancel();
                    RequestReturned -= handler;
                }
            };
            RequestReturned += handler;
            await Task.Run(() => { source.Token.WaitHandle.WaitOne(); });

            return Health;
        }

        public async Task<BoardStateUpdate> GetBoardState()
        {
            RequestBoardState();

            using CancellationTokenSource source = new CancellationTokenSource();
            EventHandler<ClientRequestReturnedEventArgs>? handler = null;
            handler = (s, e) =>
            {
                if (handler != null && e.DataType == DataType.BoardStateUpdate)
                {
                    source.Cancel();
                    RequestReturned -= handler;
                }
            };
            RequestReturned += handler;
            await Task.Run(() => { source.Token.WaitHandle.WaitOne(); });

            return BoardState;
        }

        public async Task<BoardIdUpdate> GetBoardId()
        {
            RequestBoardId();

            using CancellationTokenSource source = new CancellationTokenSource();
            EventHandler<ClientRequestReturnedEventArgs>? handler = null;
            handler = (s, e) =>
            {
                if (handler != null && e.DataType == DataType.BoardIdUpdate)
                {
                    source.Cancel();
                    RequestReturned -= handler;
                }
            };
            RequestReturned += handler;
            await Task.Run(() => { source.Token.WaitHandle.WaitOne(); });

            return BoardId;
        }

        internal void FireRequestReturned(object? sender, ClientRequestReturnedEventArgs eventArgs)
        {
            RequestReturned?.Invoke(this, eventArgs);
        }

        internal void FireUpdateReceieved(object? sender, ClientUpdateReceivedEventArgs eventArgs)
        {
            UpdateReceieved?.Invoke(this, eventArgs);
        }
    }

    public class ClientRequestReturnedEventArgs : EventArgs
    {
        public Client Client { get; }
        public DataType DataType { get; }

        public ClientRequestReturnedEventArgs(Client client, DataType dataType)
        {
            Client = client;
            DataType = dataType;
        }
    }

    public class ClientUpdateReceivedEventArgs : EventArgs
    {
        public Client Client { get; }
        public DataType DataType { get; }
        public object Data { get; }

        public ClientUpdateReceivedEventArgs(Client client, DataType dataType, object data)
        {
            Client = client;
            DataType = dataType;
            Data = data;
        }

        public ClientUpdateReceivedEventArgs(Client client, YPRUpdate update)
        {
            Client = client;
            DataType = DataType.YPRUpdate;
            Data = update;
        }

        public ClientUpdateReceivedEventArgs(Client client, RawUpdate update)
        {
            Client = client;
            DataType = DataType.RawUpdate;
            Data = update;
        }

        public ClientUpdateReceivedEventArgs(Client client, AHRSUpdate update)
        {
            Client = client;
            DataType = DataType.AHRSUpdate;
            Data = update;
        }

        public ClientUpdateReceivedEventArgs(Client client, AHRSPosUpdate update)
        {
            Client = client;
            DataType = DataType.AHRSPosUpdate;
            Data = update;
        }
    }
}
