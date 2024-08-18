using piconavx.ui.graphics;
using System.Numerics;

namespace piconavx.ui.controllers
{
    public class LiveClientPreviewController : Controller
    {
        private Dictionary<Client, AHRSPosUpdate> lastUpdates = [];
        private Client? client;
        public Client? Client { get; set; }

        public Transform Target { get; }

        public LiveClientPreviewController(Transform target)
        {
            Target = target;
        }

        public override void Subscribe()
        {
            UIServer.ClientDisconnected += new PrioritizedAction<GenericPriority, Client>(GenericPriority.Medium, Server_ClientDisconnected);
            UIServer.ClientUpdate += new PrioritizedAction<GenericPriority, Client, AHRSPosUpdate>(GenericPriority.Medium, Server_ClientUpdate);
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            UIServer.ClientDisconnected -= Server_ClientDisconnected;
            UIServer.ClientUpdate -= Server_ClientUpdate;
            Scene.Update -= Scene_Update;
        }

        private void Server_ClientDisconnected(Client client)
        {
            lastUpdates.Remove(client);
        }

        private void Server_ClientUpdate(Client client, AHRSPosUpdate update)
        {
            if (client == this.client)
            {
                if (!lastUpdates.TryAdd(client, update))
                    lastUpdates[client] = update;
            }
        }

        private void Scene_Update(double deltaTime)
        {
            if (Client != client)
            {
                client = Client;
            }

            if (client != null)
            {
                var lastUpdate = lastUpdates.GetValueOrDefault(client);
                Target.Rotation = new Quaternion((float)lastUpdate.QuatX, (float)lastUpdate.QuatZ, -(float)lastUpdate.QuatY, (float)lastUpdate.QuatW);
                Target.Position = new Vector3(10*(float)lastUpdate.DispX, 10 * (float)lastUpdate.DispZ, 10 * -(float)lastUpdate.DispY);
            } else
            {
                Target.Rotation = Quaternion.Identity;
                Target.Position = Vector3.Zero;
            }
        }
    }
}
