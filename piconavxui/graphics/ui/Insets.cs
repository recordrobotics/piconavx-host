using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public struct Insets
    {
        public float Left; public float Top; public float Right; public float Bottom;

        public readonly float Horizontal => Left + Right;
        public readonly float Vertical => Top + Bottom;

        public Insets(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Insets(float all) : this(all, all, all, all)
        { }
    }
}
