using FontStashSharp;
using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public abstract class TextInputBase : UIController
    {
        private List<Glyph> glyphs = [];
        protected List<Glyph> Glyphs { get => glyphs; set => glyphs = value; }

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
        }

        private void Scene_Update(double delta)
        {

        }

        private void Repeater_KeyDown(Key key)
        {
            Debug.Write(key);
        }

        private void Repeater_Char(char key)
        {
            Debug.Write(key);
        }

        private void Scene_KeyDown(Key key)
        {
            var repeater = new EventRepeater<GenericPriority, Key>(key);
            repeater.Event += new PrioritizedAction<GenericPriority, Key>(GenericPriority.Medium, Repeater_KeyDown);
            Scene.KeyUp += repeater.Stop(GenericPriority.Medium, Scene.KeyUp, true);
        }
    }
}
