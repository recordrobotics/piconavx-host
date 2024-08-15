using FontStashSharp;
using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public abstract class TextInputBase : UIController
    {
        private List<Glyph> glyphs = [];
        protected List<Glyph> Glyphs { get => glyphs; set => glyphs = value; }

        public int Cursor { get; set; } = 0;

        protected TextInputBase(Canvas canvas) : base(canvas)
        {
        }

        public override void Subscribe()
        {
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            Scene.KeyDown += new PrioritizedAction<GenericPriority, Key>(GenericPriority.Medium, Scene_KeyDown);
            Scene.KeyChar += new PrioritizedAction<GenericPriority, char>(GenericPriority.Medium, Repeater_Char);
        }

        public override void Unsubscribe()
        {
            Scene.Update -= Scene_Update;
            Scene.KeyDown -= Scene_KeyDown;
            Scene.KeyChar -= Repeater_Char;
        }

        protected Rectangle GetGlyphRectAt(int index)
        {
            if (index < glyphs.Count)
            {
                return glyphs[index].Bounds;
            }
            else if (index < 0 && glyphs.Count > 0)
            {
                return new(glyphs[0].Bounds.X - glyphs[0].Bounds.Width, glyphs[0].Bounds.Y, glyphs[0].Bounds.Width, glyphs[0].Bounds.Height);
            }
            else if (glyphs.Count > 0)
            {
                return new(glyphs[^1].Bounds.Right, glyphs[^1].Bounds.Y, glyphs[^1].Bounds.Width, glyphs[^1].Bounds.Height);
            }
            else
            {
                return Rectangle.Empty;
            }
        }

        protected abstract void InvalidateGlyphs();
        protected abstract void AddChar(char chr, int index);
        protected abstract void RemoveChars(int index, int length);

        private void Scene_Update(double delta)
        {

        }

        private void Repeater_KeyDown(Key key)
        {
            switch (key)
            {
                case Key.Backspace:
                    {
                        if (Cursor > 0)
                            RemoveChars(--Cursor, 1);
                    }
                    break;
            }
        }

        private void Repeater_Char(char key)
        {
            AddChar(key, Cursor++);
        }

        private void Scene_KeyDown(Key key)
        {
            var repeater = new EventRepeater<GenericPriority, Key>(key);
            repeater.Event += new PrioritizedAction<GenericPriority, Key>(GenericPriority.Medium, Repeater_KeyDown);
            Scene.KeyUp += repeater.Stop(GenericPriority.Medium, Scene.KeyUp, true);
        }
    }
}
