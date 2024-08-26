using piconavx.ui.controllers;
using System.Drawing;

namespace piconavx.ui.graphics.ui
{
    public class SplitButton : FlowPanel
    {
        private (FlowPanel panel, Image background, AnchorLayout backgroundLayout, Image border, AnchorLayout borderLayout, Label label)[] options;

        private static Texture? SideBg;
        private static Texture? SideFg;
        private static Texture? SideBgFlip;
        private static Texture? SideFgFlip;
        private static Texture? MidBg;
        private static Texture? MidFg;

        public int SelectedIndex { get; set; } = 0;
        public string? SelectedText { get => SelectedIndex >= 0 && SelectedIndex < options.Length ? options[SelectedIndex].label.Text : null; }

        public PrioritizedList<PrioritizedAction<GenericPriority>> SelectionChanged = new();

        public void NotifySelectionChanged()
        {
            foreach (var action in SelectionChanged)
            {
                action.Action.Invoke();
            }
        }

        public SplitButton(Canvas canvas, int selectedIndex, params string[] options) : base(canvas)
        {
            SelectedIndex = selectedIndex;
            Direction = FlowDirection.Horizontal;
            AlignItems = AlignItems.Middle;

            if (SideBg == null)
            {
                SideBg = Scene.AddResource(new Texture("assets/textures/split_sbg.png")
                {
                    Border = new Insets(21, 21, 3, 21),
                    WrapMode = TextureWrapMode.Clamp
                });

                SideBg.UpdateSettings();
            }

            if (SideFg == null)
            {
                SideFg = Scene.AddResource(new Texture("assets/textures/split_sfg.png")
                {
                    Border = new Insets(21, 21, 5, 21),
                    WrapMode = TextureWrapMode.Clamp
                });

                SideFg.UpdateSettings();
            }

            if (SideBgFlip == null)
            {
                SideBgFlip = SideBg.CloneMutated();
                SideBgFlip.Border = new Insets(21, 21, 21, 21);
            }

            if (SideFgFlip == null)
            {
                SideFgFlip = SideFg.CloneMutated();
                SideFgFlip.Border = new Insets(21, 21, 21, 21);
            }

            if (MidBg == null)
            {
                MidBg = Scene.AddResource(new Texture("assets/textures/split_mbg.png")
                {
                    Border = new Insets(21, 21, 3, 21),
                    WrapMode = TextureWrapMode.Clamp
                });

                MidBg.UpdateSettings();
            }

            if (MidFg == null)
            {
                MidFg = Scene.AddResource(new Texture("assets/textures/split_mfg.png")
                {
                    Border = new Insets(21, 21, 5, 21),
                    WrapMode = TextureWrapMode.Clamp
                });

                MidFg.UpdateSettings();
            }

            this.options = new (FlowPanel panel, Image background, AnchorLayout backgroundLayout, Image border, AnchorLayout borderLayout, Label label)[options.Length];

            for (int i = 0; i < options.Length; i++)
            {
                FlowPanel panel = new FlowPanel(canvas);
                panel.Direction = FlowDirection.Horizontal;
                panel.Padding = new Insets(22.5f, 12f, 22.5f, 12f);

                Image background = new Image(canvas);
                background.Color = SelectedIndex == i ? Theme.Primary.Background : Theme.Outline.Background;
                background.HitTestAlphaClip = 0.9f;
                background.Texture = i == 0 ? SideBg : i == options.Length - 1 ? SideBgFlip : MidBg;
                background.ImageType = ImageType.Sliced;
                background.Size = new Size(12, 12);
                if (i < options.Length - 1)
                    background.SizeAlt = new Size(0, 0);
                background.FlipHorizontal = i == options.Length - 1;
                background.RaycastTransparency = RaycastTransparency.Transparent;

                background.Click += CreateClickHandler(i);

                AnchorLayout backgroundLayout = new AnchorLayout(background, panel);
                backgroundLayout.Anchor = Anchor.All;
                backgroundLayout.Insets = new Insets(0);

                Image border = new Image(canvas);
                border.Color = SelectedIndex == i ? Theme.Primary.Border : Theme.Outline.Border;
                border.Texture = i == 0 ? SideFg : i == options.Length - 1 ? SideFgFlip : MidFg;
                border.ImageType = ImageType.Sliced;
                border.Size = new Size(12, 12);
                if (i < options.Length - 1)
                    border.SizeAlt = new Size(0, 0);
                border.FlipHorizontal = i == options.Length - 1;
                border.RaycastTransparency = RaycastTransparency.Hidden;

                AnchorLayout borderLayout = new AnchorLayout(border, panel);
                borderLayout.Anchor = Anchor.All;
                borderLayout.Insets = new Insets(0);

                Label label = new Label(options[i], canvas);
                label.FontSize = 13;
                label.Color = SelectedIndex == i ? Theme.Primary.Text : Theme.Outline.Text;
                panel.Components.Add(label);

                this.options[i] = (panel, background, backgroundLayout, border, borderLayout, label);

                Components.Add(panel);
            }

            UpdateZIndex();
        }

        private PrioritizedAction<GenericPriority> CreateClickHandler(int index)
        {
            return new PrioritizedAction<GenericPriority>(GenericPriority.Highest, () => ClickHandler(index));
        }

        protected override void UpdateZIndex()
        {
            foreach ((FlowPanel panel, Image background, _, Image border, _, Label label) in options)
            {
                panel.ZIndex = ZIndex;
                background.ZIndex = ZIndex;
                border.ZIndex = ZIndex + 2;
                label.ZIndex = ZIndex + 1;
            }
        }

        public override void OnAdd()
        {
            base.OnAdd();
            foreach ((FlowPanel panel, Image background, _, Image border, _, Label label) in options)
            {
                Canvas.AddComponent(panel);
                Canvas.AddComponent(background);
                Canvas.AddComponent(border);
                Canvas.AddComponent(label);
            }
        }

        public override void OnRemove()
        {
            base.OnRemove();
            foreach ((_, Image background, _, Image border, _, _) in options)
            {
                Scene.InvokeLater(background.Unsubscribe, DeferralMode.NextEvent); // Destroy as soon as possible
                Scene.InvokeLater(border.Unsubscribe, DeferralMode.NextEvent);
                Canvas.RemoveComponent(background);
                Canvas.RemoveComponent(border);
            }
        }

        public override void Subscribe()
        {
            base.Subscribe();
            foreach ((_, _, AnchorLayout backgroundLayout, _, AnchorLayout borderLayout, _) in options)
            {
                backgroundLayout.Subscribe();
                borderLayout.Subscribe();
            }

            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.AfterGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            foreach ((_, _, AnchorLayout backgroundLayout, _, AnchorLayout borderLayout, _) in options)
            {
                backgroundLayout.Unsubscribe();
                borderLayout.Unsubscribe();
            }

            Scene.Update -= Scene_Update;
        }

        private void Scene_Update(double deltaTime)
        {
            for (int i = 0; i < options.Length; i++)
            {
                if (i != SelectedIndex)
                {
                    options[i].border.Color = Theme.Outline.Border;
                    options[i].background.Color = options[i].background.MouseDown ? Theme.Outline.BackgroundActive : options[i].background.MouseOver ? Theme.Outline.BackgroundHover : Theme.Outline.Background;
                    options[i].label.Color = Theme.Outline.Text;
                } else
                {
                    options[i].border.Color = Theme.Primary.Border;
                    options[i].background.Color = Theme.Primary.Background;
                    options[i].label.Color = Theme.Primary.Text;
                }
            }
        }

        private void ClickHandler(int index)
        {
            SelectedIndex = index;
            NotifySelectionChanged();
        }
    }
}
