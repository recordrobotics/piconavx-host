using piconavx.ui.controllers;
using SixLabors.ImageSharp.ColorSpaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using static piconavx.ui.graphics.ui.RichTextSegmentation;

namespace piconavx.ui.graphics.ui
{
    public class ClientDetails : FlowPanel
    {
        private Dictionary<Client, ClientUpdate?> lastUpdates = [];
        private Client? client;
        public Client? Client { get; set; }

        private static Texture? refreshIcon;
        private static Texture? restoreIcon;
        private static Texture? zeroIcon;

        private ConcurrentBag<Controller> unsubscribeList = [];

        public ClientDetails(Canvas canvas) : base(canvas)
        {
            Gap = 15;
            refreshIcon ??= Scene.AddResource(new Texture("assets/textures/refresh.png"));
            restoreIcon ??= Scene.AddResource(new Texture("assets/textures/restore.png"));
            zeroIcon ??= Scene.AddResource(new Texture("assets/textures/zero.png"));
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

        private void AddHeader(string name, bool first = false)
        {
            FlowPanel row = new FlowPanel(Canvas);
            row.Direction = FlowDirection.Horizontal;
            row.AlignItems = AlignItems.Middle;
            row.Gap = 6;
            if(!first)
                row.Padding = new Insets(0, 30, 0, 0);

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

        private void AddHeader(string name, UIController second, bool first = false)
        {
            FlowPanel cont = new FlowPanel(Canvas);
            cont.Direction = FlowDirection.Horizontal;
            cont.AlignItems = AlignItems.Middle;
            cont.Stretch = true;
            cont.AutoSize = FlowLayout.AutoSize.Y;
            if (!first)
                cont.Padding = new Insets(0, 30, 0, 0);

            AnchorLayout contLayout = new AnchorLayout(cont, VirtualWorkingRectangle);
            contLayout.Anchor = Anchor.Left | Anchor.Right;
            contLayout.Insets = new Insets(0);
            unsubscribeList.Add(contLayout);

            FlowPanel row = new FlowPanel(Canvas);
            row.Direction = FlowDirection.Horizontal;
            row.AlignItems = AlignItems.Middle;
            row.Gap = 6;

            cont.Components.Add(row);
            cont.Components.Add(second);

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

            Canvas.AddComponent(cont);
            Canvas.AddComponent(row);
            Canvas.AddComponent(label);
            Canvas.AddComponent(refreshBtn);
            Components.Add(cont);
            cont.ZIndex = ZIndex;
            row.ZIndex = ZIndex;
            label.ZIndex = ZIndex;
            refreshBtn.ZIndex = ZIndex;
            Scene.InvokeLater(cont.Subscribe, DeferralMode.NextFrame); // NextFrame because we want them to render after update, but NextEvent is just render
            Scene.InvokeLater(contLayout.Subscribe, DeferralMode.NextFrame);
        }

        private FlowPanel AddSection()
        {
            FlowPanel row = new FlowPanel(Canvas);
            row.Direction = FlowDirection.Vertical;
            row.Gap = 15;
            row.Padding = new Insets(30, 0, 30, 0);
            row.AutoSize = FlowLayout.AutoSize.Y;
            AnchorLayout layout = new AnchorLayout(row, VirtualWorkingRectangle);
            layout.Anchor = Anchor.Left | Anchor.Right;
            layout.Insets = new Insets(0);
            unsubscribeList.Add(layout);

            Canvas.AddComponent(row);
            Components.Add(row);
            row.ZIndex = ZIndex;
            Scene.InvokeLater(row.Subscribe, DeferralMode.NextFrame); // NextFrame because we want them to render after update, but NextEvent is just render
            Scene.InvokeLater(layout.Subscribe, DeferralMode.NextFrame);

            return row;
        }

        private void AddLabel(FlowPanel section, Func<(string, TextSegment[]?)> textDelegate)
        {
            Label label = new Label(textDelegate, Canvas);
            label.Color = Theme.Text;
            label.FontSize = 13;
            Canvas.AddComponent(label);
            section.Components.Add(label);
            label.ZIndex = ZIndex;
        }

        private void AddStatusIndicator(FlowPanel row, string text, Func<bool> statusDelegate)
        {
            StatusIndicator status = new StatusIndicator(text, statusDelegate, Canvas);
            Canvas.AddComponent(status);
            row.Components.Add(status);
            status.ZIndex = ZIndex;
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
                AddHeader("Device", true);

                var section = AddSection();

                AddLabel(section,
                    () => Segment(
                        $"{TextSecondary}Endpoint:{Default} {(((IPEndPoint?)client?.Tcp?.Client.RemoteEndPoint)?.Address)?.ToString() ?? "<UNKNOWN>"}{TextSecondary}:{(((IPEndPoint?)client?.Tcp?.Client.RemoteEndPoint)?.Port)?.ToString() ?? "<UNKNOWN>"}"
                        ));

                AddLabel(section,
                    () => Segment(
                        $"{TextSecondary}Firmware: v{Default}{client?.BoardId.FwVerMajor ?? 0}.{client?.BoardId.FwVerMinor ?? 0}.0"
                        ));

                AddLabel(section,
                    () => Segment(
                        $"{TextSecondary}Board: v{Default}{string.Join('.', client?.BoardId.HwRev.ToString().PadRight(2, '0').ToCharArray() ?? [])}"
                        ));

                AddLabel(section,
                    () => Segment(
                        $"{TextSecondary}Status:{Default} {client?.BoardState.OpStatus.ToString()}"
                        ));

                AddLabel(section,
                    () => Segment(
                        $"{TextSecondary}Calibration:{Default} {client?.BoardState.CalStatus.ToString()}"
                        ));

                AddHeader("Health");

                section = AddSection();

                AddLabel(section,
                    () => Segment(
                        $"{TextSecondary}Memory:{Default} {(client == null ? "---" : (client.Health.MemoryUsed / 1024))}kB / {(client == null ? "---" : (client.Health.MemoryTotal / 1024))}kB ({(client == null ? "----" : (client.Health.MemoryTotal == 0 ? "0" : ((long)client.Health.MemoryUsed * 10000L / (long)client.Health.MemoryTotal).ToString().InsertFromEnd(2, ".")))}%)"
                        ));

                AddLabel(section,
                    () =>
                    {
                        ClientUpdate? lastUpdate = client == null ? null : lastUpdates.GetValueOrDefault(client);
                        double? temp =
                            lastUpdate?.Type == ClientUpdateType.AHRSPos ?
                            lastUpdate?.AHRSPosUpdate.MpuTemp :
                            lastUpdate?.Type == ClientUpdateType.AHRS ?
                            lastUpdate?.AHRSUpdate.MpuTemp :
                            lastUpdate?.Type == ClientUpdateType.Raw ?
                            lastUpdate?.RawUpdate.TempC : null;

                        return Segment(
                            $"{TextSecondary}Temperature:{Default} {(client == null ? "----" : client.Health.CoreTemp.ToString("N2"))} °C (Core) | {((temp == null || !temp.HasValue) ? "----" : temp.Value.ToString("N2"))} °C (Sensor)"
                            );
                    });

                FlowPanel statusRow = new FlowPanel(Canvas);
                statusRow.Direction = FlowDirection.Horizontal;
                statusRow.Gap = 15;
                statusRow.Wrap = true;
                statusRow.AutoSize = FlowLayout.AutoSize.Y;
                AnchorLayout layout = new AnchorLayout(statusRow, section.VirtualWorkingRectangle);
                layout.Anchor = Anchor.Left | Anchor.Right;
                layout.Insets = new Insets(0);
                unsubscribeList.Add(layout);
                Canvas.AddComponent(statusRow);
                section.Components.Add(statusRow);
                statusRow.ZIndex = ZIndex;
                Scene.InvokeLater(layout.Subscribe, DeferralMode.NextFrame);

                AddStatusIndicator(statusRow, "Gyroscope", () => client?.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.GyroPassed) ?? false);
                AddStatusIndicator(statusRow, "Accelerometer", () => client?.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.AccelPassed) ?? false);
                AddStatusIndicator(statusRow, "Compass", () => client?.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.MagPassed) ?? false);
                AddStatusIndicator(statusRow, "Barometer", () => client?.BoardState.SelftestStatus.HasFlag(NavXSelftestStatus.BaroPassed) ?? false);
                AddStatusIndicator(statusRow, "Moving", () => client?.BoardState.SensorStatus.HasFlag(NavXSensorStatus.Moving) ?? false);
                AddStatusIndicator(statusRow, "Yaw Stable", () => client?.BoardState.SensorStatus.HasFlag(NavXSensorStatus.YawStable) ?? false);
                AddStatusIndicator(statusRow, "Altitude Valid", () => client?.BoardState.SensorStatus.HasFlag(NavXSensorStatus.AltitudeValid) ?? false);
                AddStatusIndicator(statusRow, "Sea Level", () => client?.BoardState.SensorStatus.HasFlag(NavXSensorStatus.SealevelPressSet) ?? false);
                AddStatusIndicator(statusRow, "Fused Heading Valid", () => client?.BoardState.SensorStatus.HasFlag(NavXSensorStatus.FusedHeadingValid) ?? false);
                AddStatusIndicator(statusRow, "Magnetic Disturbance", () => client?.BoardState.SensorStatus.HasFlag(NavXSensorStatus.MagDisturbance) ?? false);


                SplitButton button = new SplitButton(Canvas, ClientDataTypeIndex(Client.DataType), "AHRS+", "AHRS", "YPR", "Raw");
                AddHeader("Data", button);
                button.ZIndex = ZIndex;
                Canvas.AddComponent(button);

                section = AddSection();

                UpdateList updateList = new UpdateList(Canvas);
                section.Components.Add(updateList);
                updateList.ZIndex = ZIndex;
                updateList.Client = Client;
                Canvas.AddComponent(updateList);

                button.SelectionChanged += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, () =>
                {
                    Client.SetDataType(ClientIndexToDataType(button.SelectedIndex));
                });
            }
        }

        private int ClientDataTypeIndex(HostSetDataType dataType)
        {
            switch (dataType)
            {
                case HostSetDataType.AHRSPos:
                    return 0;
                case HostSetDataType.AHRS:
                    return 1;
                case HostSetDataType.YPR:
                    return 2;
                case HostSetDataType.Raw:
                    return 3;
                default:
                    return -1;
            }
        }

        private HostSetDataType ClientIndexToDataType(int index)
        {
            switch (index)
            {
                case 0:
                    return HostSetDataType.AHRSPos;
                case 1:
                    return HostSetDataType.AHRS;
                case 2:
                    return HostSetDataType.YPR;
                case 3:
                    return HostSetDataType.Raw;
                default:
                    return HostSetDataType.AHRSPos;
            }
        }

        private Stopwatch sw_hifreq = Stopwatch.StartNew();
        private Stopwatch sw_lowfreq = Stopwatch.StartNew();
        private Stopwatch sw_tick = Stopwatch.StartNew();

        private void Server_ClientUpdate(Client client, ClientUpdate update)
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
