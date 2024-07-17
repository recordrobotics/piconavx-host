using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public struct UIVertex : ITessellatorVertex<UIVertex>
    {
        public Vector2 position;
        public Rgba32 color;
        public Vector2 texCoords;

        public Vector2 Position { readonly get => position; set => position = value; }
        public Rgba32 Color { readonly get => color; set => color = value; }
        public Vector2 TexCoords { readonly get => texCoords; set => texCoords = value; }
        public readonly Vector2 TexCoordsAlt { get => default; set { } }

        public static UIVertex Create(Vector2 position)
        {
            return new()
            {
                position = position
            };
        }

        public static UIVertex Create(Vector2 position, Rgba32 color)
        {
            return new()
            {
                position = position,
                color = color
            };
        }

        public static UIVertex Create(Vector2 position, Rgba32 color, Vector2 texCoords)
        {
            return new()
            {
                position = position,
                color = color,
                texCoords = texCoords
            };
        }

        public static UIVertex Create(Vector2 position, Rgba32 color, Vector2 texCoords, Vector2 texCoordsAlt)
        {
            return new()
            {
                position = position,
                color = color,
                texCoords = texCoords
            };
        }

        public static void SetUpVertexAttributePointers(VertexArrayObject<UIVertex, short> vao)
        {
            vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 1, false, 0);
            vao.VertexAttributePointer(1, 4, VertexAttribPointerType.UnsignedByte, 1, true, 2 * sizeof(float));
            vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 1, false, 2 * sizeof(float) + 4 * sizeof(byte));
        }
    }

    public struct UIVertexExt : ITessellatorVertex<UIVertexExt>
    {
        public Vector2 position;
        public Rgba32 color;
        public Vector2 texCoords;
        public Vector2 texCoordsAlt;

        public Vector2 Position { readonly get => position; set => position = value; }
        public Rgba32 Color { readonly get => color; set => color = value; }
        public Vector2 TexCoords { readonly get => texCoords; set => texCoords = value; }
        public Vector2 TexCoordsAlt { readonly get => texCoordsAlt; set => texCoordsAlt = value; }

        public static UIVertexExt Create(Vector2 position)
        {
            return new()
            {
                position = position
            };
        }

        public static UIVertexExt Create(Vector2 position, Rgba32 color)
        {
            return new()
            {
                position = position,
                color = color
            };
        }

        public static UIVertexExt Create(Vector2 position, Rgba32 color, Vector2 texCoords)
        {
            return new()
            {
                position = position,
                color = color,
                texCoords = texCoords
            };
        }

        public static UIVertexExt Create(Vector2 position, Rgba32 color, Vector2 texCoords, Vector2 texCoordsAlt)
        {
            return new()
            {
                position = position,
                color = color,
                texCoords = texCoords,
                texCoordsAlt = texCoordsAlt
            };
        }

        public static void SetUpVertexAttributePointers(VertexArrayObject<UIVertexExt, short> vao)
        {
            vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 1, false, 0);
            vao.VertexAttributePointer(1, 4, VertexAttribPointerType.UnsignedByte, 1, true, 2 * sizeof(float));
            vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 1, false, 2 * sizeof(float) + 4 * sizeof(byte));
            vao.VertexAttributePointer(3, 2, VertexAttribPointerType.Float, 1, false, 2 * sizeof(float) + 4 * sizeof(byte) + 2 * sizeof(float));
        }
    }
}
