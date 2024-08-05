using piconavx.ui.controllers;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;

namespace piconavx.ui.graphics.ui
{
    public class SettingsPage : Page
    {
        public readonly Rgba32 BACKGROUND = Rgba32.ParseHex("#0F0F0F");
        public readonly Rgba32 HEADER = Rgba32.ParseHex("#FFF");

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

        private InputField inputField;
        private AnchorLayout inputFieldLayout;

        public SettingsPage(Canvas canvas, Navigator navigator) : base(canvas, navigator)
        {
            background = new Image(canvas);
            background.Color = BACKGROUND;
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            headerPanel = new Panel(canvas);
            headerPanel.Bounds = new RectangleF(0, 0, 0, 140);
            headerPanelLayout = new AnchorLayout(headerPanel, this);
            headerPanelLayout.Anchor = Anchor.TopLeft | Anchor.Right;
            headerPanelLayout.Insets = new Insets(0);

            headerBackground = new Image(canvas);
            headerBackground.Color = BACKGROUND;
            headerBackgroundLayout = new AnchorLayout(headerBackground, headerPanel);
            headerBackgroundLayout.Anchor = Anchor.All;
            headerBackgroundLayout.Insets = new Insets(0);

            header = new Label("Server Options", canvas);
            header.FontSize = 24.5f;
            header.Font = FontFace.InterSemiBold;
            header.Color = new FontStashSharp.FSColor(HEADER.ToVector4());
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
            saveButton.Color = Button.ButtonColor.Primary;
            saveButton.FontSize = 15;
            saveButton.RenderOffset = new System.Numerics.Vector2(0, 2);
            saveButton.AutoSize = Button.AutoSizeMode.TextAndIcon;
            saveButton.Padding = new Insets(35, 17, 35, 17);
            controlPanel.Components.Add(saveButton);
            saveButton.SetTooltip("Save changes");

            cancelButton = new Button("Cancel", canvas);
            cancelButton.FontSize = 15;
            cancelButton.RenderOffset = new System.Numerics.Vector2(0, 2);
            cancelButton.AutoSize = Button.AutoSizeMode.TextAndIcon;
            cancelButton.Padding = new Insets(35, 17, 35, 17);
            cancelButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, Navigator.Back);
            controlPanel.Components.Add(cancelButton);
            cancelButton.SetTooltip("Discard changes");

            inputField = new InputField(canvas);

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
            inputField.ZIndex = ZIndex + 9;
        }

        public override void Show()
        {
            SubscribeLater(
                background, backgroundAnchor,
                headerPanel, headerPanelLayout,
                header, headerLayout,
                headerBackground, headerBackgroundLayout,
                controlPanel, controlPanelLayout,
                inputField
                );

            Canvas.AddComponent(background);
            Canvas.AddComponent(headerPanel);
            Canvas.AddComponent(headerBackground);
            Canvas.AddComponent(header);
            Canvas.AddComponent(controlPanel);
            Canvas.AddComponent(saveButton);
            Canvas.AddComponent(cancelButton);
            Canvas.AddComponent(inputField);

            UpdateZIndex();
        }

        public override void Hide()
        {
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(headerPanel);
            Canvas.RemoveComponent(headerBackground);
            Canvas.RemoveComponent(header);
            Canvas.RemoveComponent(controlPanel);
            Canvas.RemoveComponent(inputField);
            UnsubscribeLater(
                background, backgroundAnchor,
                headerPanel, headerPanelLayout,
                headerBackground, headerBackgroundLayout,
                header, headerLayout,
                controlPanel, controlPanelLayout,
                inputField
                );
        }
    }
}
