using piconavx.ui.controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public class ClientDetails : FlowPanel
    {
        private AHRSPosUpdate lastUpdate;
        private Client? client;
        private Texture stopIcon;
        private Texture recordIcon;
        public Client? Client { get; set; }

        public ClientDetails(Canvas canvas) : base(canvas)
        {
            Gap = 3;
            stopIcon = Scene.AddResource(new Texture("assets/textures/stop.png"));
            recordIcon = Scene.AddResource(new Texture("assets/textures/record.png"));
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

        private void AddLabel(Func<string> textDelegate)
        {
            Label label = new Label(textDelegate, Canvas);
            Canvas.AddComponent(label);
            Components.Add(label);
            label.ZIndex = ZIndex;
            Scene.InvokeLater(label.Subscribe, DeferralMode.NextFrame); // NextFrame because we want them to render after update, but NextEvent is just render
        }

        private Button? startRecordingButton;
        private Button? stopRecordingButton;

        public void InvalidateComponents()
        {
            foreach (var component in Components)
            {
                Scene.InvokeLater(component.Unsubscribe, DeferralMode.NextEvent); // Destroy as soon as possible
                Canvas.RemoveComponent(component);
            }

            startRecordingButton = stopRecordingButton = null;
            Components.Clear();

            if (Client != null)
            {
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
                AddLabel(() => "Temperature: " + client?.Health.CoreTemp.ToString("N2") + "°c (Core) | " + lastUpdate.MpuTemp.ToString("N2") + "°c (Sensor)" + ((client?.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.BaroPassed) ?? false) ? (" | " + lastUpdate.BaroTemp.ToString("N2") + "°c (Baro)") : ""));
                AddLabel(() => "Yaw: " + lastUpdate.Yaw + "\nPitch: " + lastUpdate.Pitch + "\nRoll: " + lastUpdate.Roll);

                FlowPanel recordingRow = new FlowPanel(Canvas);
                Canvas.AddComponent(recordingRow);
                Components.Add(recordingRow);
                recordingRow.ZIndex = ZIndex;
                recordingRow.Direction = FlowDirection.Horizontal;
                recordingRow.AlignItems = AlignItems.Middle;
                recordingRow.Gap = 10;
                recordingRow.Padding = new Insets(0, 6, 0, 6);
                Scene.InvokeLater(recordingRow.Subscribe, DeferralMode.NextFrame);

                startRecordingButton = new Button("Start Recording", Canvas);
                Canvas.AddComponent(startRecordingButton);
                recordingRow.Components.Add(startRecordingButton);
                startRecordingButton.ZIndex = ZIndex;
                startRecordingButton.Disabled = client?.DataType == HostSetDataType.Feed;
                startRecordingButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Medium, () =>
                {
                    client?.SetDataType(HostSetDataType.Feed);
                });
                startRecordingButton.Icon = recordIcon;
                startRecordingButton.IsPrimary = true;
                Scene.InvokeLater(startRecordingButton.Subscribe, DeferralMode.NextFrame);

                stopRecordingButton = new Button("Stop Recording", Canvas);
                stopRecordingButton.IsIconButton = true;
                stopRecordingButton.Padding = new Insets(12);
                Canvas.AddComponent(stopRecordingButton);
                recordingRow.Components.Add(stopRecordingButton);
                stopRecordingButton.ZIndex = ZIndex;
                stopRecordingButton.Disabled = client?.DataType != HostSetDataType.Feed;
                stopRecordingButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Medium, () =>
                {
                    client?.SetDataType(HostSetDataType.AHRSPos);
                });
                stopRecordingButton.Icon = stopIcon;
                Scene.InvokeLater(stopRecordingButton.Subscribe, DeferralMode.NextFrame);
            }
        }

        private Stopwatch sw_hifreq = Stopwatch.StartNew();
        private Stopwatch sw_lowfreq = Stopwatch.StartNew();
        private Stopwatch sw_tick = Stopwatch.StartNew();

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

            if (startRecordingButton != null)
                startRecordingButton.Disabled = client?.DataType == HostSetDataType.Feed;
            if (stopRecordingButton != null)
                stopRecordingButton.Disabled = client?.DataType != HostSetDataType.Feed;
        }
    }
}
