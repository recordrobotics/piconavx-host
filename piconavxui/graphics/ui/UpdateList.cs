using piconavx.ui.controllers;
using System.Collections.Concurrent;
using static piconavx.ui.graphics.ui.RichTextSegmentation;

namespace piconavx.ui.graphics.ui
{
    public class UpdateList : FlowPanel
    {
        private Dictionary<Client, ClientUpdate?> lastUpdates = [];
        private Client? client;
        public Client? Client { get; set; }

        private ConcurrentBag<Controller> unsubscribeList = [];

        public UpdateList(Canvas canvas) : base(canvas)
        {
            Direction = FlowDirection.Vertical;
            Gap = 7.5f;
        }

        public override void Subscribe()
        {
            UIServer.ClientUpdate += new PrioritizedAction<GenericPriority, Client, ClientUpdate>(GenericPriority.Medium, Server_ClientUpdate);
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            base.Subscribe();
        }

        public override void Unsubscribe()
        {
            UIServer.ClientUpdate -= Server_ClientUpdate;
            Scene.Update -= Scene_Update;
            base.Unsubscribe();
        }

        private void AddLabel(Func<(string, TextSegment[]?)> textDelegate)
        {
            Label label = new Label(textDelegate, Canvas);
            label.Color = Theme.Text;
            label.FontSize = 13;
            Canvas.AddComponent(label);
            Components.Add(label);
            label.ZIndex = ZIndex;
        }

        public void InvalidateComponents()
        {
            foreach (var component in Components)
            {
                Scene.InvokeLater(component.Unsubscribe, DeferralMode.NextEvent); // Destroy as soon as possible
                Canvas.RemoveComponent(component);
            }

            while (unsubscribeList.TryTake(out var component))
            {
                Scene.InvokeLater(component.Unsubscribe, DeferralMode.NextEvent); // Destroy as soon as possible
            }

            Components.Clear();

            if(Client != null)
            {
                switch(Client.DataType)
                {
                    case HostSetDataType.AHRSPos:
                        {

                        }
                        break;
                }

                AddLabel(() => Segment($"{TextSecondary}Yaw:{Default} {(client == null ? "---" : lastUpdates.GetValueOrDefault(client)?.AHRSPosUpdate.Yaw.ToString() ?? "---")}"));
            }
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
                InvalidateComponents();
            }
        }
    }
}
