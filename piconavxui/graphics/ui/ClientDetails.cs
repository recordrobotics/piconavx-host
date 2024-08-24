﻿using piconavx.ui.controllers;
using System.Diagnostics;
using System.Drawing;

namespace piconavx.ui.graphics.ui
{
    public class ClientDetails : FlowPanel
    {
        private Dictionary<Client, AHRSPosUpdate> lastUpdates = [];
        private Client? client;
        public Client? Client { get; set; }

        private static Texture? refreshIcon;
        private static Texture? restoreIcon;
        private static Texture? zeroIcon;

        public ClientDetails(Canvas canvas) : base(canvas)
        {
            Gap = 15;
            refreshIcon ??= Scene.AddResource(new Texture("assets/textures/refresh.png"));
            restoreIcon ??= Scene.AddResource(new Texture("assets/textures/restore.png"));
            zeroIcon ??= Scene.AddResource(new Texture("assets/textures/zero.png"));
        }

        public override void Subscribe()
        {
            UIServer.ClientUpdate += new PrioritizedAction<GenericPriority, Client, AHRSPosUpdate>(GenericPriority.Medium, Server_ClientUpdate);
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            base.Subscribe();
        }

        public override void Unsubscribe()
        {
            UIServer.ClientUpdate -= Server_ClientUpdate;
            Scene.Update -= Scene_Update;
            base.Unsubscribe();
        }

        private void AddHeader(string name)
        {
            FlowPanel row = new FlowPanel(Canvas);
            row.Direction = FlowDirection.Horizontal;
            row.AlignItems = AlignItems.Middle;
            row.Gap = 6;

            Label label = new Label(name, Canvas);
            label.Font = FontFace.InterSemiBold;
            label.FontSize = 20;
            label.Color = Theme.Header;
            row.Components.Add(label);

            Button refreshBtn = new Button("Refresh", Canvas);
            refreshBtn.IsIconButton = true;
            refreshBtn.Icon = refreshIcon;
            refreshBtn.Color = Theme.TextSecondaryButton;
            refreshBtn.Padding = new Insets(0);
            refreshBtn.IconSize = new SizeF(27, 27);
            refreshBtn.SetTooltip("Refresh");
            row.Components.Add(refreshBtn);

            Canvas.AddComponent(row);
            Canvas.AddComponent(label);
            Canvas.AddComponent(refreshBtn);
            Components.Add(row);
            row.ZIndex = ZIndex;
            label.ZIndex = ZIndex;
            refreshBtn.ZIndex = ZIndex;
            Scene.InvokeLater(row.Subscribe, DeferralMode.NextFrame); // NextFrame because we want them to render after update, but NextEvent is just render
        }

        private void AddLabel(Func<string> textDelegate)
        {
            Label label = new Label(textDelegate, Canvas);
            label.Color = Theme.Text;
            Canvas.AddComponent(label);
            Components.Add(label);
            label.ZIndex = ZIndex;
            Scene.InvokeLater(label.Subscribe, DeferralMode.NextFrame); // NextFrame because we want them to render after update, but NextEvent is just render
        }

        public void InvalidateComponents()
        {
            foreach (var component in Components)
            {
                Scene.InvokeLater(component.Unsubscribe, DeferralMode.NextEvent); // Destroy as soon as possible
                Canvas.RemoveComponent(component);
            }

            Components.Clear();

            if (Client != null)
            {
                AddHeader("Device");
                AddHeader("Health");
                AddHeader("Data");

                AddLabel(() => "Connected: " + client?.Id);
                AddLabel(() => "Firmware: v" + client?.BoardId.FwVerMajor + "." + client?.BoardId.FwVerMinor + "." + client?.BoardId.FwRevision);
                AddLabel(() => "Board: v" + string.Join('.', client?.BoardId.HwRev.ToString().ToCharArray() ?? []));
                AddLabel(() => "Status: " + client?.BoardState.OpStatus.ToString() + " | Calibration: " + client?.BoardState.CalStatus.ToString());
                AddLabel(() =>
                {
                    if (client == null) return "GYRO:NO | ACCEL:NO | MAG:NO | BARO:NO";
                    bool gyro = client.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.GyroPassed);
                    bool accel = client.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.AccelPassed);
                    bool mag = client.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.MagPassed);
                    bool baro = client.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.BaroPassed);
                    return "GYRO:" + (gyro ? "OK" : "NO") + " | " + "ACCEL:" + (accel ? "OK" : "NO") + " | " + "MAG:" + (mag ? "OK" : "NO") + " | " + "BARO:" + (baro ? "OK" : "NO");
                });
                AddLabel(() =>
                {
                    if (client == null) return "Moving: NO\nAltitude Valid: NO\nFused Heading Valid: NO\nMag Disturbance: NO\nYaw Stable: NO\nSea level press set: NO";

                    bool moving = client.BoardState.SensorStatus.HasFlag(NavXSensorStatus.Moving);
                    bool altitudeValid = client.BoardState.SensorStatus.HasFlag(NavXSensorStatus.AltitudeValid);
                    bool fusedHeadingValid = client.BoardState.SensorStatus.HasFlag(NavXSensorStatus.FusedHeadingValid);
                    bool magDisturbance = client.BoardState.SensorStatus.HasFlag(NavXSensorStatus.MagDisturbance);
                    bool yawStable = client.BoardState.SensorStatus.HasFlag(NavXSensorStatus.YawStable);
                    bool sealevelPressSet = client.BoardState.SensorStatus.HasFlag(NavXSensorStatus.SealevelPressSet);
                    return "Moving: " + (moving ? "YES" : "NO") + "\n" +
                    "Altitude Valid: " + (altitudeValid ? "YES" : "NO") + "\n" +
                    "Fused Heading Valid: " + (fusedHeadingValid ? "YES" : "NO") + "\n" +
                    "Mag Disturbance: " + (magDisturbance ? "YES" : "NO") + "\n" +
                    "Yaw Stable: " + (yawStable ? "YES" : "NO") + "\n" +
                    "Sea level press set: " + (sealevelPressSet ? "YES" : "NO");
                });
                AddLabel(() => "Memory: " + client?.Health.MemoryUsed + "B / " + client?.Health.MemoryTotal + "B (" + MathF.Round((float)(client?.Health.MemoryUsed ?? 0) / (client?.Health.MemoryTotal ?? 1) * 100f, 2).ToString("N2") + "%)");
                AddLabel(() =>
                {
                    var lastUpdate = client == null ? default : lastUpdates.GetValueOrDefault(client);
                    return "Temperature: " + client?.Health.CoreTemp.ToString("N2") + "°c (Core) | " + lastUpdate.MpuTemp.ToString("N2") + "°c (Sensor)" + ((client?.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.BaroPassed) ?? false) ? (" | " + lastUpdate.BaroTemp.ToString("N2") + "°c (Baro)") : "");
                });
                AddLabel(() =>
                {
                    var lastUpdate = client == null ? default : lastUpdates.GetValueOrDefault(client);
                    return "Yaw: " + lastUpdate.Yaw + "\nPitch: " + lastUpdate.Pitch + "\nRoll: " + lastUpdate.Roll;
                });

                FlowPanel recordingRow = new FlowPanel(Canvas);
                Canvas.AddComponent(recordingRow);
                Components.Add(recordingRow);
                recordingRow.ZIndex = ZIndex;
                recordingRow.Direction = FlowDirection.Horizontal;
                recordingRow.AlignItems = AlignItems.Middle;
                recordingRow.Gap = 10;
                recordingRow.Padding = new Insets(0, 6, 0, 6);
                Scene.InvokeLater(recordingRow.Subscribe, DeferralMode.NextFrame);
            }
        }

        private Stopwatch sw_hifreq = Stopwatch.StartNew();
        private Stopwatch sw_lowfreq = Stopwatch.StartNew();
        private Stopwatch sw_tick = Stopwatch.StartNew();

        private void Server_ClientUpdate(Client client, AHRSPosUpdate update)
        {
            if (client == this.client)
            {
                if (!lastUpdates.TryAdd(client, update))
                    lastUpdates[client] = update;
            }
        }

        public override void OnAdd()
        {
            base.OnAdd();
            InvalidateComponents();
        }

        private void Scene_Update(double deltaTime)
        {
            if (Client != client)
            {
                client = Client;
                InvalidateComponents();
            }

            if (client != null && client.BoardState.UpdateRateHz > 0)
            {
                int updateTime = 1000 / client.BoardState.UpdateRateHz;

                if (sw_hifreq.ElapsedMilliseconds > 4 * updateTime) // request every 4th update
                {
                    sw_hifreq.Restart();
                    client?.RequestBoardState();
                }

                if (sw_lowfreq.ElapsedMilliseconds > 25 * updateTime) // request every 25th update
                {
                    sw_lowfreq.Restart();
                    client?.RequestBoardId();
                }

                if (sw_tick.ElapsedMilliseconds > 50 * updateTime) // request every 50th update
                {
                    sw_tick.Restart();
                    client?.RequestHealth();
                }
            }
        }
    }
}
