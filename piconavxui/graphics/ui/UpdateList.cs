using piconavx.ui.controllers;
using System.Collections.Concurrent;
using static piconavx.ui.graphics.ui.RichTextSegmentation;

namespace piconavx.ui.graphics.ui
{
    public class UpdateList : FlowPanel
    {
        private Dictionary<Client, ClientUpdate?> lastUpdates = [];
        private ClientUpdateType lastUpdateType;
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

        public override void OnAdd()
        {
            base.OnAdd();
            InvalidateComponents();
        }

        private void AddLabel(Func<(string, TextSegment[]?)> textDelegate)
        {
            Label label = new Label(textDelegate, Canvas);
            label.Color = Theme.Text;
            label.FontSize = 13;
            Canvas.AddComponent(label);
            Components.Add(label);
            label.ZIndex = ZIndex;
            Scene.InvokeLater(label.Subscribe, DeferralMode.NextFrame);
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

            if (Client != null)
            {
                switch (Client.DataType)
                {
                    case HostSetDataType.AHRSPos:
                        {
                            AddLabel(() => Segment($"{TextSecondary}Yaw:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.Yaw.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Pitch:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.Pitch.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Roll:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.Roll.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Compass Heading:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.CompassHeading.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Fused Heading:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.FusedHeading.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Altitude:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.Altitude.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Pressure:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.BarometricPressure.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Rotation:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.QuatX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.QuatY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.QuatZ.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}W:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.QuatW.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Acceleration:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.LinearAccelX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.LinearAccelY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.LinearAccelZ.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Velocity:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.VelX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.VelY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.VelZ.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Displacement:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.DispX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.DispY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSPosUpdate.DispZ.ToString() ?? "---"}"));
                        }
                        break;
                    case HostSetDataType.AHRS:
                        {
                            AddLabel(() => Segment($"{TextSecondary}Yaw:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.Yaw.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Pitch:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.Pitch.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Roll:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.Roll.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Compass Heading:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.CompassHeading.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Fused Heading:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.FusedHeading.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Altitude:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.Altitude.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Pressure:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.BarometricPressure.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Rotation:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.QuatX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.QuatY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.QuatZ.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}W:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.QuatW.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Acceleration:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.LinearAccelX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.LinearAccelY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.LinearAccelZ.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Calibrated Compass:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.CalMagX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.CalMagY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.CalMagZ.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Raw Compass:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.RawMagX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.RawMagY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.RawMagZ.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Magnetic Field Ratio:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.MagFieldNormRatio.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Magnetic Field Scalar:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.AHRSUpdate.MagFieldNormScalar.ToString() ?? "---"}"));
                        }
                        break;
                    case HostSetDataType.YPR:
                        {
                            AddLabel(() => Segment($"{TextSecondary}Yaw:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.YPRUpdate.Yaw.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Pitch:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.YPRUpdate.Pitch.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Roll:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.YPRUpdate.Roll.ToString() ?? "---"}"));
                        }
                        break;
                    case HostSetDataType.Raw:
                        {
                            AddLabel(() => Segment($"{TextSecondary}Gyroscope:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.RawUpdate.GyroX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.RawUpdate.GyroY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.RawUpdate.GyroZ.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Acceleration:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.RawUpdate.AccelX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.RawUpdate.AccelY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.RawUpdate.AccelZ.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"{TextSecondary}Compass:"));
                            AddLabel(() => Segment($"    {TextSecondary}X:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.RawUpdate.MagX.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Y:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.RawUpdate.MagY.ToString() ?? "---"}"));
                            AddLabel(() => Segment($"    {TextSecondary}Z:{Default} {lastUpdates.GetValueOrDefaultNullable(client)?.RawUpdate.MagZ.ToString() ?? "---"}"));
                        }
                        break;
                }
            }
        }

        private void Server_ClientUpdate(Client client, ClientUpdate update)
        {
            if (client == this.client)
            {
                if (!lastUpdates.TryAdd(client, update))
                    lastUpdates[client] = update;

                if (update.Type != lastUpdateType)
                {
                    Scene.InvokeLater(InvalidateComponents, DeferralMode.NextFrame);
                }

                lastUpdateType = update.Type;
            }
        }

        private void Scene_Update(double deltaTime)
        {
            if (Client != client)
            {
                lastUpdateType = Client?.DataType.ToClientUpdateType() ?? ClientUpdateType.AHRSPos;
                client = Client;
                InvalidateComponents();
            }
        }
    }
}
