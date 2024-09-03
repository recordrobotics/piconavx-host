using piconavx.ui.graphics;
using System.Numerics;

namespace piconavx.ui.controllers
{
    public class LiveClientPreviewController : Controller
    {
        private Dictionary<Client, ClientUpdate> lastUpdates = [];
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
            UIServer.ClientUpdate += new PrioritizedAction<GenericPriority, Client, ClientUpdate>(GenericPriority.Medium, Server_ClientUpdate);
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

        private void Server_ClientUpdate(Client client, ClientUpdate update)
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
                double dispX = 0;
                double dispY = 0;
                double dispZ = 0;

                double quatX = 0;
                double quatY = 0;
                double quatZ = 0;
                double quatW = 0;

                switch (lastUpdate.Type)
                {
                    case ClientUpdateType.AHRSPos:
                        {
                            dispX = lastUpdate.AHRSPosUpdate.DispX;
                            dispY = lastUpdate.AHRSPosUpdate.DispY;
                            dispZ = lastUpdate.AHRSPosUpdate.DispZ;
                            quatX = lastUpdate.AHRSPosUpdate.QuatX;
                            quatY = lastUpdate.AHRSPosUpdate.QuatY;
                            quatZ = lastUpdate.AHRSPosUpdate.QuatZ;
                            quatW = lastUpdate.AHRSPosUpdate.QuatW;
                            break;
                        }
                    case ClientUpdateType.AHRS:
                        {
                            quatX = lastUpdate.AHRSUpdate.QuatX;
                            quatY = lastUpdate.AHRSUpdate.QuatY;
                            quatZ = lastUpdate.AHRSUpdate.QuatZ;
                            quatW = lastUpdate.AHRSUpdate.QuatW;
                            break;
                        }
                    case ClientUpdateType.YPR:
                        {
                            var quat = Quaternion.CreateFromYawPitchRoll((float)lastUpdate.YPRUpdate.Yaw, (float)lastUpdate.YPRUpdate.Pitch, (float)lastUpdate.YPRUpdate.Roll);
                            quatX = quat.X;
                            quatY = quat.Y;
                            quatZ = quat.Z;
                            quatW = quat.W;
                            break;
                        }
                }

                Target.Rotation = new Quaternion((float)quatX, (float)quatZ, -(float)quatY, (float)quatW);
                Target.Position = new Vector3(10 * (float)dispX, 10 * (float)dispZ, 10 * -(float)dispY);
            }
            else
            {
                Target.Rotation = Quaternion.Identity;
                Target.Position = Vector3.Zero;
            }
        }
    }
}
