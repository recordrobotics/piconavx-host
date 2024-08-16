using FontStashSharp;
using piconavx.ui.controllers;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Numerics;
using System.Text;
using static piconavx.ui.graphics.ui.Button;

namespace piconavx.ui.graphics.ui
{
    public class InputField : TextInputBase
    {
        public enum CursorBlinkMode
        {
            Hidden,
            Static,
            Blink,
            Pulse
        }

        public struct AutoSizeData(float lineHeight)
        {
            public float lineHeight = lineHeight;
        }

        public CursorBlinkMode BlinkMode { get; set; } = CursorBlinkMode.Blink;
        public double BlinkInterval { get; set; } = 0.5;

        public ButtonColor BackgroundColor { get; set; } = ButtonColor.Neutral;

        private int zIndex = 0;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                background.ZIndex = zIndex - 1;
                Canvas.InvalidateHierarchy();
            }
        }

        private StringBuilder text;
        public StringBuilder Text
        {
            get => text; set { text = value; InvalidateGlyphs(true); }
        }

        private float fontSize = 12.0f;
        public float FontSize
        {
            get => fontSize; set { fontSize = value; InvalidateGlyphs(false); }
        }

        private FontFace font = FontFace.InterRegular;
        public FontFace Font { get => font; set { font = value; InvalidateGlyphs(false); } }

        private bool autoSize = true;
        public bool AutoSize { get => autoSize; set => autoSize = value; }

        private bool autoSizeMultiline = false;
        public bool AutoSizeMultiline { get => autoSizeMultiline; set => autoSizeMultiline = value; }

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set { bounds = value; InvalidateGlyphs(false); } }

        private RectangleF contentBounds;
        public override RectangleF ContentBounds { get => contentBounds; }

        private Insets padding = new Insets(20, 16, 20, 16);
        public Insets Padding { get => padding; set => padding = value; }

        private Vector2 renderOffset;
        public Vector2 RenderOffset { get => renderOffset; set { renderOffset = value; InvalidateGlyphs(false); } }

        private FSColor color;
        public FSColor Color
        {
            get => color; set => color = value;
        }

        private Rgba32 selectionColor;
        public Rgba32 SelectionColor
        {
            get => selectionColor; set => selectionColor = value;
        }

        private bool overrideInputFocused = false;
        public override bool InputFocused { get => overrideInputFocused || Canvas.LastTarget == background; set => overrideInputFocused = value; }

        public override RaycastTransparency RaycastTransparency { get => base.RaycastTransparency; set => base.RaycastTransparency = background.RaycastTransparency = value; }

        public override bool MouseOver { get => background.MouseOver; set => _ = value; }
        public override bool MouseDown { get => background.MouseDown; set => _ = value; }

        public override int SelectionStart { get; set; } = 0;
        public override int SelectionLength { get; set; } = 0;

        private Image background;
        private AnchorLayout backgroundAnchor;

        public InputField(string text, Canvas canvas) : base(canvas)
        {
            background = new Image(canvas);
            background.Transform = Transform;
            background.ZIndex = ZIndex - 1; // background
            background.HitTestAlphaClip = 0.9f;
            background.Color = BackgroundColor.Background;
            background.Texture = Texture.RoundedRect;
            background.ImageType = ImageType.Sliced;
            background.Size = new Size(15, 15);
            background.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, () =>
            {
                if (!Disabled) // Disabled handling
                    NotifyClick();
            });
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            RaycastTransparency = RaycastTransparency.Transparent;

            this.text = new(text);
            color = FSColor.White;
            selectionColor = Rgba32.ParseHex("#1857ba");
            bounds = GetAutoSizeBounds(out _);
            InvalidateGlyphs(true);
        }

        public RectangleF GetAutoSizeBounds(out AutoSizeData data)
        {
            var fontSystem = Window.FontSystems[this.font];
            var font = fontSystem.GetFont(fontSize);

            // Get measured glyph bounds
            Vector2 size = font.MeasureString(text, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));

            // Set additional data
            data = new AutoSizeData(font.LineHeight * fontSystem.FontResolutionFactor);

            // Overwrite measured height with a stable line-height based size
            size.Y = GetLineCount() * data.lineHeight;

            contentBounds = new RectangleF(bounds.X + padding.Left, bounds.Y + padding.Top, size.X, size.Y);
            return new RectangleF(bounds.X, bounds.Y, contentBounds.Width + padding.Horizontal, contentBounds.Height + padding.Vertical);
        }

        protected override float GetLineHeight()
        {
            GetAutoSizeBounds(out var data);
            return data.lineHeight;
        }

        public override void Render(double deltaTime, RenderProperties properties)
        {
            var fontSystem = Window.FontSystems[this.font];
            var font = fontSystem.GetFont(fontSize);

            float lineHeight = GetLineHeight();

            if (SelectionLength > 0)
            {
                int end = SelectionStart + SelectionLength;
                int row = 0;
                int xs = (int)contentBounds.X;
                int xe = (int)contentBounds.X;

                UIMaterial.ColorMaterial.Use(properties, Transform);

                for (int i = 0; i < Glyphs.Count && i < end; i++)
                {
                    xe = Glyphs[i].Bounds.Right;

                    if (i == SelectionStart)
                    {
                        xs = Glyphs[i].Bounds.Left;
                    }

                    if (i < Glyphs.Count && Glyphs[i].Codepoint == NEW_LINE_CODEPOINT)
                    {
                        if (i >= SelectionStart)
                        {
                            Tessellator.Quad.DrawQuad(new RectangleF(xs, contentBounds.Y + row * lineHeight, xe - xs, lineHeight), selectionColor);
                        }

                        row++;
                        xs = (int)contentBounds.X;
                        xe = (int)contentBounds.X;
                    }
                }

                if (xe > (int)contentBounds.X)
                {
                    Tessellator.Quad.DrawQuad(new RectangleF(xs, contentBounds.Y + row * lineHeight, xe - xs, lineHeight), selectionColor);
                }

                Tessellator.Quad.Flush();
            }

            Window.FontRenderer.Begin(Transform.Matrix);
            font.DrawText(Window.FontRenderer, text, new Vector2(contentBounds.X, contentBounds.Y), color, 0, renderOffset, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
            Window.FontRenderer.End();

            if (InputFocused && !Disabled && cursorVisible)
            {
                var cursorRect = GetGlyphRectAt(Cursor);
                var cursorBounds = cursorRect.bounds.AsFloat();

                // Snap cursor to left bound if new line
                if (cursorRect.isNewLine || cursorRect.bounds.IsEmpty)
                {
                    cursorBounds.X = contentBounds.X;
                }

                // Set cursor Y to current line
                cursorBounds.Y = contentBounds.Y + lineHeight * cursorRect.row;

                cursorBounds.Width = 1;
                cursorBounds.Height = lineHeight;

                UIMaterial.ColorMaterial.Use(properties, Transform);
                var colorVec = color.ToVector4();
                colorVec.W = cursorOpacity;
                Tessellator.Quad.DrawQuad(cursorBounds, new Rgba32(colorVec));
                Tessellator.Quad.Flush();
            }
        }

        public override void Subscribe()
        {
            base.Subscribe();
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            background.Subscribe();
            backgroundAnchor.Subscribe();
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            Scene.Update -= Scene_Update;
            background.Unsubscribe();
            backgroundAnchor.Unsubscribe();
        }

        public override void OnAdd()
        {
            base.OnAdd();
            Canvas.AddComponent(background);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            Canvas.RemoveComponent(background);
        }

        double cursorTimer = 0;
        bool cursorVisible = false;
        float cursorOpacity = 1;

        private void Scene_Update(double deltaTime)
        {
            if (autoSize || autoSizeMultiline)
            {
                var size = GetAutoSizeBounds(out var data);

                if (autoSize)
                {
                    bounds = new RectangleF(bounds.X, bounds.Y, size.Width, data.lineHeight + padding.Vertical);
                }

                if (autoSizeMultiline)
                {
                    bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width, size.Height);
                }
            }

            Transform.UpdateCache();
            background.Color = Disabled ? BackgroundColor.BackgroundDisabled : (MouseOver || InputFocused) ? BackgroundColor.BackgroundHover : BackgroundColor.Background;

            if (InputFocused && !Disabled)
            {
                switch (BlinkMode)
                {
                    case CursorBlinkMode.Hidden:
                        cursorVisible = false;
                        cursorTimer = 0;
                        cursorOpacity = 1;
                        break;
                    case CursorBlinkMode.Static:
                        cursorVisible = true;
                        cursorTimer = 0;
                        cursorOpacity = 1;
                        break;
                    case CursorBlinkMode.Blink:
                        cursorOpacity = 1;
                        cursorTimer += deltaTime;
                        if (cursorTimer >= BlinkInterval)
                        {
                            cursorTimer = 0;
                            cursorVisible = !cursorVisible;
                        }
                        break;
                    case CursorBlinkMode.Pulse:
                        cursorVisible = true;
                        cursorTimer += deltaTime;
                        if (cursorTimer >= BlinkInterval * 2)
                        {
                            cursorTimer = 0;
                        }

                        double norm = cursorTimer / BlinkInterval;
                        if (norm > 1)
                            norm = 2 - norm;

                        cursorOpacity = (float)(1 - norm);
                        break;
                }
            }
            else
            {
                InvalidateCursor();
            }
        }

        protected override void AddChar(char chr, int index)
        {
            if (index < text.Length)
                text.Insert(index, chr);
            else
                text.Append(chr);
            InvalidateGlyphs(true);
        }

        protected override void RemoveChars(int index, int length)
        {
            text.Remove(index, length);
            InvalidateGlyphs(true);
        }

        protected override string GetChars(int index, int length)
        {
            return text.ToString().Substring(index, length);
        }

        protected override void AddString(string str, int index)
        {
            if (index < text.Length)
                text.Insert(index, str);
            else
                text.Append(str);
            InvalidateGlyphs(true);
        }

        protected override void InvalidateCursor()
        {
            cursorVisible = true;
            cursorTimer = 0;
            cursorOpacity = 1;
        }

        protected override void InvalidateGlyphs(bool contentsModified)
        {
            if (contentsModified)
            {
                InvalidateCursor();
            }

            var fontSystem = Window.FontSystems[this.font];
            var font = fontSystem.GetFont(fontSize);

            Glyphs = font.GetGlyphs(text, new Vector2(contentBounds.X, contentBounds.Y), renderOffset, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
        }
    }
}
