using piconavx.ui.graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.controllers
{
    public class FeedClientPreviewController : Controller
    {
        private Client? client;
        public Client? Client { get; set; }

        public Model Target { get; }

        public FeedClientPreviewController(Model target)
        {
            Target = target;
        }

        public override void Subscribe()
        {
            UIServer.ClientFeedUpdate += new PrioritizedAction<GenericPriority, Client, FeedChunk[]>(GenericPriority.Medium, Server_ClientFeedUpdate);
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            UIServer.ClientFeedUpdate -= Server_ClientFeedUpdate;
            Scene.Update -= Scene_Update;
        }

        private void Server_ClientFeedUpdate(Client client, FeedChunk[] chunks)
        {
            if (client == this.client)
            {
                Scene.InvokeLater(() =>
                {
                    Target.Transforms = new Transform[UIServer.GetClientFeed(client)?.Count??0];
                }, DeferralMode.NextFrame);
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
                var feed = UIServer.GetClientFeed(client);
                if (feed != null)
                {
                    for (int i = 0; i < Math.Min(feed.Count, Target.Transforms.Length); i++)
                    {
                        if (Target.Transforms[i] == null)
                            Target.Transforms[i] = new Transform();
                        FeedChunk chunk = feed[i];
                        Target.Transforms[i]!.Rotation = new Quaternion((float)chunk.Data.QuatX, (float)chunk.Data.QuatZ, -(float)chunk.Data.QuatY, (float)chunk.Data.QuatW);
                        Target.Transforms[i]!.Position = new Vector3(10*(float)chunk.Data.DispX, 10 * (float)chunk.Data.DispZ, 10 * -(float)chunk.Data.DispY);
                    }
                }
            }
            else
            {
                Target.Transforms = [];
            }
        }
    }
}
