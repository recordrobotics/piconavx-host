using piconavx.ui.controllers;
using Silk.NET.Input.Sdl;
using System.Diagnostics;
using System.Drawing;

namespace piconavx.ui.graphics.ui
{
    public class SettingsPage : Page
    {
        private Image background;
        private AnchorLayout backgroundAnchor;

        private Panel headerPanel;
        private AnchorLayout headerPanelLayout;

        private Image headerBackground;
        private AnchorLayout headerBackgroundLayout;

        private Label header;
        private AnchorLayout headerLayout;

        private FlowPanel controlPanel;
        private AnchorLayout controlPanelLayout;

        private Button saveButton;
        private Button cancelButton;

        private ScrollPanel settingsListContainer;
        private AnchorLayout settingsListContainerLayout;

        private FlowPanel settingsList;
        private AnchorLayout settingsListLayout;

        public SettingsPage(Canvas canvas, Navigator navigator) : base(canvas, navigator)
        {
            background = new Image(canvas);
            background.Color = Theme.Background;
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            headerPanel = new Panel(canvas);
            headerPanel.Bounds = new RectangleF(0, 0, 0, 140);
            headerPanelLayout = new AnchorLayout(headerPanel, this);
            headerPanelLayout.Anchor = Anchor.TopLeft | Anchor.Right;
            headerPanelLayout.Insets = new Insets(0);

            headerBackground = new Image(canvas);
            headerBackground.Color = Theme.Background;
            headerBackgroundLayout = new AnchorLayout(headerBackground, headerPanel);
            headerBackgroundLayout.Anchor = Anchor.All;
            headerBackgroundLayout.Insets = new Insets(0);

            header = new Label("Server Options", canvas);
            header.FontSize = 24.5f;
            header.Font = FontFace.InterSemiBold;
            header.Color = Theme.Header;
            headerLayout = new AnchorLayout(header, headerPanel);
            headerLayout.Anchor = Anchor.TopLeft | Anchor.Bottom;
            headerLayout.AllowResize = false;
            headerLayout.Insets = new Insets(53, 0, 0, 0);

            controlPanel = new FlowPanel(canvas);
            controlPanel.Direction = FlowDirection.Horizontal;
            controlPanel.AlignItems = AlignItems.Middle;
            controlPanel.Gap = 27;
            controlPanelLayout = new AnchorLayout(controlPanel, headerPanel);
            controlPanelLayout.Anchor = Anchor.Top | Anchor.Right | Anchor.Bottom;
            controlPanelLayout.AllowResize = false;
            controlPanelLayout.Insets = new Insets(0, 0, 51, 0);

            saveButton = new Button("Save", canvas);
            saveButton.Color = Theme.Primary;
            saveButton.FontSize = 15;
            saveButton.RenderOffset = new System.Numerics.Vector2(0, 2);
            saveButton.AutoSize = Button.AutoSizeMode.TextAndIcon;
            saveButton.Padding = new Insets(35, 17, 35, 17);
            controlPanel.Components.Add(saveButton);
            saveButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, () =>
            {
                SavedResource.Settings.Current.Address = serverIp?.Text.ToString() ?? "0.0.0.0";

                if (int.TryParse(serverPort?.Text.ToString() ?? "65432", out var p))
                {
                    SavedResource.Settings.Current.Port = p;
                }

                if (int.TryParse(clientTimeout?.Text.ToString() ?? "1000", out p))
                {
                    SavedResource.Settings.Current.Timeout = p;
                }

                if (int.TryParse(clientHighBandwidthTimeout?.Text.ToString() ?? "10000", out p))
                {
                    SavedResource.Settings.Current.HighTimeout = p;
                }

                SavedResource.Settings.Current.Theme = theme?.Text.ToString();
                Theme.UpdateTheme();

                if (SavedResource.WriteSettings(SavedResource.Settings.Current))
                {
                    Alert.CreateOneShot("Settings successfully saved!", "File written to '" + Path.Join(SavedResource.SavePath, "settings.json") + "'.", canvas).Color = Theme.Success;
                } else
                {
                    Alert.CreateOneShot("Could not save settings!", "An error occurred while trying to write to\n'" + Path.Join(SavedResource.SavePath, "settings.json") + "'.", canvas).Color = Theme.Error;
                }
            });
            saveButton.SetTooltip("Save changes");

            cancelButton = new Button("Close", canvas);
            cancelButton.FontSize = 15;
            cancelButton.RenderOffset = new System.Numerics.Vector2(0, 2);
            cancelButton.AutoSize = Button.AutoSizeMode.TextAndIcon;
            cancelButton.Padding = new Insets(35, 17, 35, 17);
            cancelButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, Navigator.Back);
            controlPanel.Components.Add(cancelButton);
            cancelButton.SetTooltip("Close Settings");

            settingsList = new FlowPanel(canvas);
            settingsList.AutoSize = FlowLayout.AutoSize.Y;
            settingsList.Direction = FlowDirection.Vertical;
            settingsList.Padding = new Insets(0, 20, 0, 40);
            settingsList.Gap = 60;

            settingsListContainer = new ScrollPanel(canvas, settingsList);
            settingsListContainerLayout = new AnchorLayout(settingsListContainer, this);
            settingsListContainerLayout.Anchor = Anchor.All;
            settingsListContainerLayout.Insets = new Insets(52.5f, 140, 52.5f, 0);

            settingsListLayout = new AnchorLayout(settingsList, settingsListContainer.VirtualWorkingRectangle);
            settingsListLayout.Anchor = Anchor.Left | Anchor.Right;
            settingsListLayout.Insets = new Insets(0);

            UpdateZIndex();
        }

        protected override void UpdateZIndex()
        {
            background.ZIndex = ZIndex;
            headerBackground.ZIndex = ZIndex + 8;
            header.ZIndex = ZIndex + 9;
            controlPanel.ZIndex = ZIndex + 9;
            saveButton.ZIndex = ZIndex + 9;
            cancelButton.ZIndex = ZIndex + 9;
            settingsListContainer.ZIndex = ZIndex + 2;

            foreach (var component in settingsList.Components)
            {
                component.ZIndex = ZIndex + 3;
            }
        }

        class InputEntry : FlowPanel
        {
            private readonly Label header;
            private readonly Label text;
            private readonly InputField input;
            private readonly FlowPanel namePanel;

            public InputField InputField => input;

            public InputEntry(string header, string text, string value, Canvas canvas) : base(canvas)
            {
                Direction = FlowDirection.Horizontal;
                AlignItems = AlignItems.Middle;
                Gap = 40;

                namePanel = new FlowPanel(canvas);
                namePanel.Direction = FlowDirection.Vertical;
                namePanel.Gap = 20;

                this.header = new Label(header, canvas);
                this.header.FontSize = 18;
                this.header.Color = Theme.Header;

                this.text = new Label(text, canvas);
                this.text.FontSize = 13;
                this.text.Color = Theme.Text;

                namePanel.Components.Add(this.header);
                namePanel.Components.Add(this.text);

                input = new InputField(value, canvas);
                input.FontSize = 20;

                Components.Add(namePanel);
                Components.Add(input);
            }

            protected override void UpdateZIndex()
            {
                base.UpdateZIndex();
                header.ZIndex = ZIndex;
                text.ZIndex = ZIndex;
                namePanel.ZIndex = ZIndex;
                input.ZIndex = ZIndex;
            }

            public override void OnAdd()
            {
                base.OnAdd();
                Canvas.AddComponent(header);
                Canvas.AddComponent(text);
                Canvas.AddComponent(namePanel);
                Canvas.AddComponent(input);
            }
        }

        private InputField AddInput(string header, string text, string value)
        {
            var entry = new InputEntry(header, text, value, Canvas);
            settingsList.Components.Add(entry);
            Canvas.AddComponent(entry);
            return entry.InputField;
        }

        private InputField? serverIp;
        private InputField? serverPort;
        private InputField? clientTimeout;
        private InputField? clientHighBandwidthTimeout;
        private InputField? theme;

        public override void Show()
        {
            SubscribeLater(
                background, backgroundAnchor,
                headerPanel, headerPanelLayout,
                header, headerLayout,
                headerBackground, headerBackgroundLayout,
                controlPanel, controlPanelLayout,
                settingsListContainer, settingsListContainerLayout,
                settingsList, settingsListLayout
                );

            Canvas.AddComponent(background);
            Canvas.AddComponent(headerPanel);
            Canvas.AddComponent(headerBackground);
            Canvas.AddComponent(header);
            Canvas.AddComponent(controlPanel);
            Canvas.AddComponent(saveButton);
            Canvas.AddComponent(cancelButton);
            Canvas.AddComponent(settingsListContainer);
            Canvas.AddComponent(settingsList);

            settingsList.Components.Clear();

            serverIp = AddInput("Server bind address",
                "This specifies the local IP address to which to\n" +
                "bind the server. Setting this to '0.0.0.0' will allow\n" +
                "connections from any interface.", SavedResource.Settings.Current.Address ?? "0.0.0.0");
            serverIp.AutoSize = false;
            serverIp.MaxLength = 16;
            serverIp.Bounds = serverIp.GetAutoSizeBounds('0', 16, out _);

            serverPort = AddInput("Server listen port",
                "This is the port where incoming connections will be accepted.\n" +
                "Make sure to configure the sensor with the correct port.", SavedResource.Settings.Current.Port.ToString());
            serverPort.AutoSize = false;
            serverPort.MaxLength = 7;
            serverPort.Filter = char.IsAsciiDigit;
            serverPort.Bounds = serverPort.GetAutoSizeBounds('0', 7, out _);

            clientTimeout = AddInput("Client connection timeout",
                "This is the maximum number of milliseconds without updates\n" +
                "from a client before it is considered dead.", SavedResource.Settings.Current.Timeout.ToString());
            clientTimeout.AutoSize = false;
            clientTimeout.MaxLength = 10;
            clientTimeout.Filter = char.IsAsciiDigit;
            clientTimeout.Bounds = clientTimeout.GetAutoSizeBounds('0', 10, out _);

            clientHighBandwidthTimeout = AddInput("Client connection timeout - High Bandwidth",
                "This is the maximum number of milliseconds without updates\n" +
                "from a client in High Bandwidth mode before it is considered dead.\n\n" +
                "Note: High Bandwidth mode purposefully lowers the update rate\n" +
                "to increase performance.", SavedResource.Settings.Current.HighTimeout.ToString());
            clientHighBandwidthTimeout.AutoSize = false;
            clientHighBandwidthTimeout.MaxLength = 10;
            clientHighBandwidthTimeout.Filter = char.IsAsciiDigit;
            clientHighBandwidthTimeout.Bounds = clientTimeout.GetAutoSizeBounds('0', 10, out _);

            theme = AddInput("Theme file",
                "Name of the theme file (json).\n\n" +
                "The built-in themes are\n" +
                "    - dark.json\n" +
                "    - light.json\n" +
                "\n" +
                "To use custom themes, put the .json file in\n'"
                +(Path.Join(SavedResource.SavePath, "themes")??"<NOT SUPPORTED>")+"'\n" +
                "and specify just the filename here.\n" +
                "Note: custom themes overwrite built-in ones.", SavedResource.Settings.Current.Theme ?? "<UNKNOWN>");

            UpdateZIndex();
        }

        public override void Hide()
        {
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(headerPanel);
            Canvas.RemoveComponent(headerBackground);
            Canvas.RemoveComponent(header);
            Canvas.RemoveComponent(controlPanel);
            Canvas.RemoveComponent(settingsList);
            Canvas.RemoveComponent(settingsListContainer);

            UnsubscribeLater(
                background, backgroundAnchor,
                headerPanel, headerPanelLayout,
                headerBackground, headerBackgroundLayout,
                header, headerLayout,
                controlPanel, controlPanelLayout,
                settingsListContainer, settingsListContainerLayout,
                settingsList, settingsListLayout
                );
        }
    }
}
