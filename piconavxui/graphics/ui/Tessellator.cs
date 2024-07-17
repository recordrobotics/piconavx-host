using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public abstract class Tessellator : IDisposable
    {
        private static readonly QuadTessellator<UIVertex> quad = new();
        public static QuadTessellator<UIVertex> Quad { get => quad; }
        private static readonly QuadTessellator<UIVertexExt> quadExt = new();
        public static QuadTessellator<UIVertexExt> QuadExt { get => quadExt; }

        public abstract void CreateResources();

        public abstract void Reset();
        public abstract void Flush();

        public abstract void Dispose();
    }

    public interface ITessellatorVertex<TSelf> where TSelf : unmanaged, ITessellatorVertex<TSelf>
    {
        public static abstract void SetUpVertexAttributePointers(VertexArrayObject<TSelf, short> vao);

        public Vector2 Position { get; set; }
        public Rgba32 Color { get; set; }
        public Vector2 TexCoords { get; set; }
        public Vector2 TexCoordsAlt { get; set; }

        public static abstract TSelf Create(Vector2 position);
        public static abstract TSelf Create(Vector2 position, Rgba32 color);
        public static abstract TSelf Create(Vector2 position, Rgba32 color, Vector2 texCoords);
        public static abstract TSelf Create(Vector2 position, Rgba32 color, Vector2 texCoords, Vector2 texCoordsAlt);
    }
}
