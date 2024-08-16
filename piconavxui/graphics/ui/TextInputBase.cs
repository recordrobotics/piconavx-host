using FontStashSharp;
using Silk.NET.Input;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace piconavx.ui.graphics.ui
{
    public abstract class TextInputBase : UIController
    {
        protected const int NEW_LINE_CODEPOINT = 10; /* \n */

        public struct GlyphRect(Rectangle bounds, int row, int column, bool isNewLine)
        {
            public Rectangle bounds = bounds;
            public int row = row;
            public int column = column;
            public bool isNewLine = isNewLine;
        }

        private List<Glyph> glyphs = [];
        protected List<Glyph> Glyphs { get => glyphs; set => glyphs = value; }

        public virtual int Cursor { get; set; } = 0;
        public virtual bool Multiline { get; set; } = false;
        public virtual bool Disabled { get; set; } = false;
        public abstract bool InputFocused { get; set; }

        public override abstract bool MouseOver { get; set; }
        public override abstract bool MouseDown { get; set; }

        public abstract int SelectionStart { get; set; }
        public abstract int SelectionLength { get; set; }

        public abstract RectangleF ContentBounds { get; }

        protected TextInputBase(Canvas canvas) : base(canvas)
        {
        }

        public override void Subscribe()
        {
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            Scene.KeyDown += new PrioritizedAction<GenericPriority, Key>(GenericPriority.Medium, Scene_KeyDown);
            Scene.KeyUp += new PrioritizedAction<GenericPriority, Key>(GenericPriority.Medium, Scene_KeyUp);
            Scene.KeyChar += new PrioritizedAction<GenericPriority, char>(GenericPriority.Medium, Repeater_Char);
        }

        public override void Unsubscribe()
        {
            Scene.Update -= Scene_Update;
            Scene.KeyDown -= Scene_KeyDown;
            Scene.KeyUp -= Scene_KeyUp;
            Scene.KeyChar -= Repeater_Char;
        }

        protected int GetGlyphAt(int row, int x)
        {
            if (row < 0)
                return 0;

            int r = 0;

            for (int i = 0; i < glyphs.Count; i++)
            {
                if (r == row && i > 0 && (glyphs[i - 1].Bounds.X <= x || glyphs[i - 1].Codepoint == NEW_LINE_CODEPOINT || i == 1) && glyphs[i].Bounds.X >= x)
                {
                    var leftDist = x - glyphs[i - 1].Bounds.X;
                    var rightDist = glyphs[i].Bounds.X - x;
                    if (leftDist < rightDist && glyphs[i - 1].Codepoint != NEW_LINE_CODEPOINT)
                        return i - 1;
                    else
                        return i;
                }

                if (i < glyphs.Count && glyphs[i].Codepoint == NEW_LINE_CODEPOINT)
                {
                    if (r == row)
                        return i;
                    r++;
                }
            }

            return glyphs.Count;
        }

        protected int GetLineCount()
        {
            int row = 1;

            for (int i = 0; i < glyphs.Count; i++)
            {
                if (i < glyphs.Count && glyphs[i].Codepoint == NEW_LINE_CODEPOINT)
                {
                    row++;
                }
            }

            return row;
        }

        protected GlyphRect GetGlyphRectAt(int index)
        {
            int row = 0;
            int column = -1;

            for (int i = 0; i <= index; i++)
            {
                column++;
                if (i < glyphs.Count && glyphs[i].Codepoint == NEW_LINE_CODEPOINT)
                {
                    if (i != index)
                    {
                        row++;
                        column = 0;
                    }
                }
            }

            Rectangle bounds;
            bool isNewLine;

            if (index < 0 && glyphs.Count != 0)
            {
                var glyph = glyphs[0];
                isNewLine = false;
                bounds = new Rectangle(glyph.Bounds.X - glyph.XAdvance, glyph.Bounds.Y, glyph.Bounds.Width, glyph.Bounds.Height);
            }
            else if (index >= glyphs.Count && glyphs.Count != 0)
            {
                var glyph = glyphs[^1];
                isNewLine = glyph.Codepoint == NEW_LINE_CODEPOINT;
                bounds = new Rectangle(glyph.Bounds.X + glyph.XAdvance, glyph.Bounds.Y, glyph.Bounds.Width, glyph.Bounds.Height);
            }
            else if (glyphs.Count != 0)
            {
                if (glyphs.Count > 1 && index > 0)
                    isNewLine = glyphs[index - 1].Codepoint == NEW_LINE_CODEPOINT;
                else
                    isNewLine = false;
                bounds = glyphs[index].Bounds;
            }
            else
            {
                isNewLine = false;
                bounds = Rectangle.Empty;
            }

            return new GlyphRect(bounds, row, column, isNewLine);
        }

        protected abstract float GetLineHeight();
        protected abstract void InvalidateCursor();
        protected abstract void InvalidateGlyphs(bool contentsModified);
        protected abstract void AddChar(char chr, int index);
        protected abstract void RemoveChars(int index, int length);

        private void Repeater_KeyDown(Key key)
        {
            if (Disabled || !InputFocused) return;

            switch (key)
            {
                case Key.Backspace:
                    {
                        if (Cursor > 0)
                        {
                            RemoveChars(--Cursor, 1);
                            InvalidateCursor();
                        }
                    }
                    break;
                case Key.Enter:
                    {
                        if (Multiline)
                        {
                            AddChar((char)NEW_LINE_CODEPOINT, Cursor++);
                            InvalidateCursor();
                        }
                    }
                    break;
                case Key.Left:
                    {
                        if (Cursor > 0)
                        {
                            Cursor--;

                            if (modifiers.HasFlag(Modifiers.Shift))
                            {
                                if (SelectionLength == 0)
                                {
                                    SelectionLength = 1;
                                    SelectionStart = Cursor;
                                }
                                else if(SelectionStart > Cursor)
                                {
                                    SelectionStart = Cursor;
                                    SelectionLength++;
                                }
                                else
                                {
                                    SelectionLength--;
                                }
                            }
                            else
                            {
                                SelectionStart = 0;
                                SelectionLength = 0;
                            }

                            InvalidateCursor();
                        }
                    }
                    break;
                case Key.Right:
                    {
                        if (Cursor < glyphs.Count)
                        {
                            if (modifiers.HasFlag(Modifiers.Shift))
                            {
                                if (SelectionLength == 0)
                                {
                                    SelectionLength = 1;
                                    SelectionStart = Cursor;
                                }
                                else if (SelectionStart + SelectionLength > Cursor)
                                {
                                    SelectionStart++;
                                    SelectionLength--;
                                }
                                else
                                {
                                    SelectionLength++;
                                }
                            }
                            else
                            {
                                SelectionStart = 0;
                                SelectionLength = 0;
                            }

                            Cursor++;

                            InvalidateCursor();
                        }
                    }
                    break;
                case Key.Up:
                    {
                        if (Multiline)
                        {
                            var currentRect = GetGlyphRectAt(Cursor);
                            var newCursor = GetGlyphAt(currentRect.row - 1, currentRect.bounds.X);
                            if (newCursor != -1)
                            {
                                var oldCursor = Cursor;
                                Cursor = newCursor;

                                if (modifiers.HasFlag(Modifiers.Shift))
                                {
                                    if (SelectionLength == 0)
                                    {
                                        SelectionLength = oldCursor - Cursor;
                                        SelectionStart = Cursor;
                                    }
                                    else if(SelectionStart >= oldCursor)
                                    {
                                        SelectionStart = Cursor;
                                        SelectionLength += oldCursor - Cursor;
                                    } else
                                    {
                                        SelectionLength -= oldCursor - Cursor;
                                        if (SelectionLength < 0)
                                        {
                                            int oldStart = SelectionStart;
                                            int end = SelectionStart + SelectionLength;
                                            SelectionStart = end;
                                            SelectionLength = oldStart - end;
                                        }
                                    }
                                }
                                else
                                {
                                    SelectionStart = 0;
                                    SelectionLength = 0;
                                }

                                InvalidateCursor();
                            }
                        }
                    }
                    break;
                case Key.Down:
                    {
                        if (Multiline)
                        {
                            var currentRect = GetGlyphRectAt(Cursor);
                            var newCursor = GetGlyphAt(currentRect.row + 1, currentRect.bounds.X);
                            if (newCursor != -1)
                            {
                                var oldCursor = Cursor;
                                Cursor = newCursor;

                                if (modifiers.HasFlag(Modifiers.Shift))
                                {
                                    if (SelectionLength == 0)
                                    {
                                        SelectionLength = Cursor - oldCursor;
                                        SelectionStart = oldCursor;
                                    }
                                    else if (SelectionStart + SelectionLength > oldCursor)
                                    {
                                        SelectionStart += Cursor - oldCursor;
                                        SelectionLength -= Cursor - oldCursor;
                                        if (SelectionLength < 0)
                                        {
                                            int oldStart = SelectionStart;
                                            int end = SelectionStart + SelectionLength;
                                            SelectionStart = end;
                                            SelectionLength = oldStart - end;
                                        }
                                    }
                                    else
                                    {
                                        SelectionLength += Cursor - oldCursor;
                                    }
                                }
                                else
                                {
                                    SelectionStart = 0;
                                    SelectionLength = 0;
                                }

                                InvalidateCursor();
                            }
                        }
                    }
                    break;
            }
        }

        private void Repeater_Char(char key)
        {
            if (Disabled || !InputFocused) return;

            AddChar(key, Cursor++);
        }

        [Flags]
        enum Modifiers : uint
        {
            Shift = 1 << 0,
            None = 0
        }

        private Modifiers modifiers = Modifiers.None;

        private void Scene_KeyUp(Key key)
        {
            switch (key)
            {
                case Key.ShiftLeft:
                case Key.ShiftRight:
                    {
                        modifiers &= ~Modifiers.Shift;
                    }
                    break;
            }
        }

        private void Scene_KeyDown(Key key)
        {
            switch (key)
            {
                case Key.ShiftLeft:
                case Key.ShiftRight:
                    {
                        modifiers |= Modifiers.Shift;
                    }
                    break;
                default:
                    {
                        var repeater = new EventRepeater<GenericPriority, Key>(key);
                        repeater.Event += new PrioritizedAction<GenericPriority, Key>(GenericPriority.Medium, Repeater_KeyDown);
                        Scene.KeyUp += repeater.Stop(GenericPriority.Medium, Scene.KeyUp, true);
                    }
                    break;
            }
        }

        bool prevMouseDown = false;
        private void Scene_Update(double deltaTime)
        {
            if (MouseDown && !prevMouseDown)
            {
                Vector2 mouse = (Window.Current.Input?.Mice.FirstOrDefault()?.Position / GlobalScale) ?? new(0);
                int row = (int)MathF.Floor((mouse.Y - ContentBounds.Y) / GetLineHeight());
                var newCursor = GetGlyphAt(row, (int)mouse.X);
                if (newCursor != -1)
                {
                    Cursor = newCursor;
                    InvalidateCursor();
                }
            }

            prevMouseDown = MouseDown;

            if (!InputFocused || Disabled)
            {
                SelectionStart = 0;
                SelectionLength = 0;
            }
        }
    }
}
