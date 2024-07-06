using piconavx.ui.graphics;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace piconavx.ui.controllers
{
    public class UIServer : Controller
    {
        private readonly Server server;
        private Task? serverTask = null;

        public IReadOnlyList<Client> Clients => server.Clients.AsReadOnly();

        public static PrioritizedList<PrioritizedAction<GenericPriority, Client>> ClientConnected = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, Client>> ClientDisconnected = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, Client, AHRSPosUpdate>> ClientUpdate = new();

        private readonly ConcurrentQueue<(Client, AHRSPosUpdate)> clientUpdates = new();
        private readonly ConcurrentQueue<Client> clientConnects = new();
        private readonly ConcurrentQueue<Client> clientDisconnects = new();

        public UIServer(int port)
        {
            server = new Server(port);
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
        }

        public void Start()
        {
            serverTask = server.Start();
        }

        public void Shutdown()
        {
            server.Stop();
            serverTask?.Wait(500);
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
            client.ZeroYaw();
            client.ZeroDisplacement();
            client.RequestHealth();
            client.RequestBoardId();
            client.RequestBoardState();
        }

        private void Server_ClientConnected(object? sender, ClientConnectedEventArgs e)
        {
            ResetClient(e.Client);
            e.Client.UpdateReceieved += Client_UpdateReceieved;
            clientConnects.Enqueue(e.Client);
        }

        private void Client_UpdateReceieved(object? sender, ClientUpdateReceivedEventArgs e)
        {
            if (e.DataType == DataType.AHRSPosUpdate)
            {
                clientUpdates.Enqueue((e.Client, (AHRSPosUpdate)e.Data));
            }
            else
            {
                Debug.WriteLine("Unexpected data type received: " + e.DataType);
            }
        }

        private void Server_ClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
        {
            clientDisconnects.Enqueue(e.Client);
        }

        private void Scene_AppExit()
        {
            Shutdown();
        }

        private void Scene_Update(double deltaTime)
        {
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
