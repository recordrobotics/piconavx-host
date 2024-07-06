using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public abstract class Tessellator : IDisposable
    {
        private static readonly QuadTessellator quad = new();
        public static QuadTessellator Quad { get => quad; }

        public abstract void CreateResources();

        public abstract void Reset();
        public abstract void Flush();

        public abstract void Dispose();
    }
}
