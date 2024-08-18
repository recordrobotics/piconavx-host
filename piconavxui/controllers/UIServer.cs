using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace piconavx.ui.controllers
{
    public class UIServer : Controller
    {
        private readonly Server server;

        public bool Running => server.Running;
        public EndPoint? LocalEndpoint => server.LocalEndpoint;

        private static IEnumerable<IPAddress>? _GetInterfaceAddresses_cached = null;
        private static Stopwatch? _GetInterfaceAddresses_cached_sw = null;
        const long _GetInterfaceAddresses_cached_refresh = 10000;
        // https://stackoverflow.com/a/60092903
        public IEnumerable<IPAddress>? GetInterfaceAddresses()
        {
            if (_GetInterfaceAddresses_cached == null || _GetInterfaceAddresses_cached_sw == null || _GetInterfaceAddresses_cached_sw.ElapsedMilliseconds >= _GetInterfaceAddresses_cached_refresh)
            {
                _GetInterfaceAddresses_cached = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(i => i.OperationalStatus == OperationalStatus.Up && i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                    .Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(i => i.Address);
                _GetInterfaceAddresses_cached_sw ??= new();
                _GetInterfaceAddresses_cached_sw.Restart();
            }

            return _GetInterfaceAddresses_cached;
        }

        public IReadOnlyList<Client> Clients => server.Clients.AsReadOnly();

        public static PrioritizedList<PrioritizedAction<GenericPriority, Client>> ClientConnected = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, Client>> ClientDisconnected = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, Client, AHRSPosUpdate>> ClientUpdate = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, Client, FeedChunk[]>> ClientFeedUpdate = new();

        private readonly ConcurrentQueue<(Client, AHRSPosUpdate)> clientUpdates = new();
        private readonly ConcurrentQueue<(Client, FeedChunk[])> clientFeedUpdates = new();
        private readonly ConcurrentQueue<Client> clientConnects = new();
        private readonly ConcurrentQueue<Client> clientDisconnects = new();

        private static readonly Dictionary<Client, List<FeedChunk>> clientFeeds = new();

        public static IReadOnlyList<FeedChunk>? GetClientFeed(Client client)
        {
            return clientFeeds.ContainsKey(client) ? clientFeeds[client].AsReadOnly() : null;
        }

        public UIServer()
        {
            server = new Server();
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
        }

        public bool Start()
        {
            return Start(null);
        }

        public bool Start(Canvas? alertCanvas)
        {
            var ip = SavedResource.Settings.Current.Address;
            var port = SavedResource.Settings.Current.Port;

            if (IPAddress.TryParse(ip, out var ipAddress) && server.Start(ipAddress, port))
            {
                Scene.InvokeLater(() =>
                {
                    var client = server.ConnectSimulatedClient("Robot");
                    client.HighBandwidthMode = true;
                    client.BoardId = new BoardIdUpdate()
                    {
                        FwVerMajor = 3,
                        FwVerMinor = 1
                    };
                    client.BoardState = new BoardStateUpdate()
                    {
                        OpStatus = NavXOPStatus.Normal,
                        CalStatus = NavXCalStatus.Complete
                    };
                    client.Health = new HealthUpdate()
                    {
                        MemoryUsed = 111616,
                        MemoryTotal = 191488,
                        CoreTemp = 27.04
                    };

                    client = server.ConnectSimulatedClient("Speaker Note 1");
                    client.HighBandwidthMode = false;
                    client.BoardId = new BoardIdUpdate()
                    {
                        FwVerMajor = 3,
                        FwVerMinor = 1
                    };
                    client.BoardState = new BoardStateUpdate()
                    {
                        OpStatus = NavXOPStatus.Initializing,
                        CalStatus = NavXCalStatus.InProgress
                    };
                    client.Health = new HealthUpdate()
                    {
                        MemoryUsed = 44032,
                        MemoryTotal = 191488,
                        CoreTemp = 23.10
                    };

                    client = server.ConnectSimulatedClient("Speaker Note 2");
                    client.HighBandwidthMode = false;
                    client.BoardId = new BoardIdUpdate()
                    {
                        FwVerMajor = 3,
                        FwVerMinor = 1
                    };
                    client.BoardState = new BoardStateUpdate()
                    {
                        OpStatus = NavXOPStatus.Normal,
                        CalStatus = NavXCalStatus.Complete
                    };
                    client.Health = new HealthUpdate()
                    {
                        MemoryUsed = 121856,
                        MemoryTotal = 191488,
                        CoreTemp = 29.12
                    };

                    client = server.ConnectSimulatedClient("Speaker Note 3");
                    client.HighBandwidthMode = false;
                    client.BoardId = new BoardIdUpdate()
                    {
                        FwVerMajor = 3,
                        FwVerMinor = 1
                    };
                    client.BoardState = new BoardStateUpdate()
                    {
                        OpStatus = NavXOPStatus.IMUAutocalInProgress,
                        CalStatus = NavXCalStatus.InProgress
                    };
                    client.Health = new HealthUpdate()
                    {
                        MemoryUsed = 109568,
                        MemoryTotal = 191488,
                        CoreTemp = 25.20
                    };

                    client = server.ConnectSimulatedClient("Preloaded Note");
                    client.HighBandwidthMode = true;
                    client.BoardId = new BoardIdUpdate()
                    {
                        FwVerMajor = 3,
                        FwVerMinor = 1
                    };
                    client.BoardState = new BoardStateUpdate()
                    {
                        OpStatus = NavXOPStatus.Normal,
                        CalStatus = NavXCalStatus.Complete
                    };
                    client.Health = new HealthUpdate()
                    {
                        MemoryUsed = 114688,
                        MemoryTotal = 191488,
                        CoreTemp = 26.12
                    };
                }, DeferralMode.WhenAvailable);
                return true;
            }
            else if (alertCanvas != null)
            {
                Alert.CreateOneShot("Server not started!", $"There was an error binding the listener to '{ip}:{port}'", alertCanvas).Color = Theme.Error;
            }

            return false;
        }

        public void Stop()
        {
            server.Stop();
        }

        public void Shutdown()
        {
            server.ClientConnected -= Server_ClientConnected;
            server.ClientDisconnected -= Server_ClientDisconnected;

            server.Dispose();

            clientUpdates.Clear();
            clientConnects.Clear();
            clientDisconnects.Clear();
        }

        public override void Subscribe()
        {
            Scene.AppExit += new PrioritizedAction<GenericPriority>(GenericPriority.Medium, Scene_AppExit);
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            Scene.AppExit -= Scene_AppExit;
            Scene.Update -= Scene_Update;
        }

        private void ResetClient(Client client)
        {
            client.SetDataType(HostSetDataType.AHRSPos);
            client.SetFeedOverflow(HostSetFeedOverflowType.DeleteOldest);
            client.ZeroYaw();
            client.ZeroDisplacement();
            client.RequestHealth();
            client.RequestBoardId();
            client.RequestBoardState();
        }

        private void Server_ClientConnected(object? sender, ClientConnectedEventArgs e)
        {
            ResetClient(e.Client);
            clientFeeds.Add(e.Client, []);
            e.Client.UpdateReceieved += Client_UpdateReceieved;
            clientConnects.Enqueue(e.Client);
        }

        private void Client_UpdateReceieved(object? sender, ClientUpdateReceivedEventArgs e)
        {
            switch (e.DataType)
            {
                case DataType.AHRSPosUpdate:
                    {
                        clientUpdates.Enqueue((e.Client, (AHRSPosUpdate)e.Data));
                    }
                    break;
                case DataType.FeedUpdate:
                    {
                        FeedChunk[] chunks = (FeedChunk[])e.Data;
                        if (chunks.Length == 0)
                            break;
                        if (chunks[0].Data.MpuTemp > 20 && chunks[0].Data.MpuTemp < 100)
                        {
                            clientFeeds[e.Client].AddRange(chunks);
                            clientFeedUpdates.Enqueue((e.Client, chunks));
                        }
                    }
                    break;
                default:
                    {
                        Debug.WriteLine("Unexpected data type received: " + e.DataType);
                    }
                    break;
            }
        }

        private void Server_ClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
        {
            clientFeeds[e.Client].Clear();
            clientFeeds.Remove(e.Client);
            e.Client.UpdateReceieved -= Client_UpdateReceieved;
            clientDisconnects.Enqueue(e.Client);
            if (e.ReadException != null)
                Debug.WriteLine(e.ReadException);
            if (e.WriteException != null)
                Debug.WriteLine(e.WriteException);
        }

        private void Scene_AppExit()
        {
            Shutdown();
        }

        private void Scene_Update(double deltaTime)
        {
            server.ClientIdentificationTimeout = SavedResource.Settings.Current.IdentificationTimeout;
            server.ClientTimeout = SavedResource.Settings.Current.Timeout;
            server.ClientHighBandwidthTimeout = SavedResource.Settings.Current.HighTimeout;

            {
                if (clientConnects.TryDequeue(out Client? client))
                {
                    foreach (var action in ClientConnected)
                    {
                        action.Action.Invoke(client);
                    }
                }
            }
            {
                if (clientUpdates.TryDequeue(out (Client, AHRSPosUpdate) update))
                {
                    foreach (var action in ClientUpdate)
                    {
                        action.Action.Invoke(update.Item1, update.Item2);
                    }
                }
            }
            {
                if (clientFeedUpdates.TryDequeue(out (Client, FeedChunk[]) update))
                {
                    foreach (var action in ClientFeedUpdate)
                    {
                        action.Action.Invoke(update.Item1, update.Item2);
                    }
                }
            }
            {
                if (clientDisconnects.TryDequeue(out Client? client))
                {
                    foreach (var action in ClientDisconnected)
                    {
                        action.Action.Invoke(client);
                    }
                }
            }
        }
    }
}
