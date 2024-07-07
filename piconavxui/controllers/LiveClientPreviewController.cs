using piconavx.ui.graphics;

namespace piconavx.ui.controllers
{
    public class LiveClientPreviewController : Controller
    {
        private AHRSPosUpdate lastUpdate;
        private Client? client;
        public Client? Client { get; set; }

        public Transform Target { get; }

        public LiveClientPreviewController(Transform target)
        {
            Target = target;
        }

        public override void Subscribe()
        {
            UIServer.ClientUpdate += new PrioritizedAction<GenericPriority, Client, AHRSPosUpdate>(GenericPriority.Medium, Server_ClientUpdate);
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            UIServer.ClientUpdate -= Server_ClientUpdate;
            Scene.Update -= Scene_Update;
        }

        private void Server_ClientUpdate(Client client, AHRSPosUpdate update)
        {
            if (client == this.client)
            {
                lastUpdate = update;
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
                Target.Rotation = new System.Numerics.Quaternion((float)lastUpdate.QuatX, (float)lastUpdate.QuatY, (float)lastUpdate.QuatZ, (float)lastUpdate.QuatW);
                Target.Position = new System.Numerics.Vector3((float)lastUpdate.DispX, (float)lastUpdate.DispY, (float)lastUpdate.DispZ);
            } else
            {
                Target.Rotation = System.Numerics.Quaternion.Identity;
            }
        }
    }
}
