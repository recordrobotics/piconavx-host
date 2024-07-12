using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private uint _handle;
        private BufferTargetARB _bufferType;

        public unsafe BufferObject(Span<TDataType> data, BufferTargetARB bufferType, bool isDynamic)
        {
            _bufferType = bufferType;

            _handle = Window.GL.GenBuffer();
            Bind();
            fixed (TDataType* d = data)
            {
                Window.GL.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, isDynamic ? BufferUsageARB.StreamDraw : BufferUsageARB.StaticDraw);
            }
        }

        public unsafe BufferObject(TDataType data, BufferTargetARB bufferType, bool isDynamic)
        {
            _bufferType = bufferType;

            _handle = Window.GL.GenBuffer();
            Bind();
            Window.GL.BufferData(bufferType, (nuint)sizeof(TDataType), &data, isDynamic ? BufferUsageARB.StreamDraw : BufferUsageARB.StaticDraw);
        }

        public unsafe BufferObject(int size, BufferTargetARB bufferType, bool isDynamic)
        {
            _bufferType = bufferType;

            _handle = Window.GL.GenBuffer();
            Bind();

            Window.GL.BufferData(bufferType, (nuint)(size * sizeof(TDataType)), null, isDynamic ? BufferUsageARB.StreamDraw : BufferUsageARB.StaticDraw);
        }

        public unsafe void SetData(TDataType[] data, int startIndex, int elementCount)
        {
            Bind();

            fixed (TDataType* d = &data[startIndex])
            {
                Window.GL.BufferSubData(_bufferType, 0, (nuint)(elementCount * sizeof(TDataType)), d);
            }
        }

        public unsafe void SetData(TDataType data)
        {
            Bind();
            Window.GL.BufferSubData(_bufferType, 0, (nuint)sizeof(TDataType), &data);
        }

        public void Bind()
        {
            //Binding the buffer object, with the correct buffer type.
            Window.GL.BindBuffer(_bufferType, _handle);
        }

        public void BindBufferBase(uint index)
        {
            Window.GL.BindBufferBase(_bufferType, index, _handle);
        }

        public void Unbind()
        {
            Window.GL.BindBuffer(_bufferType, 0);
        }

        public void Dispose()
        {
            //Remember to delete our buffer.
            Window.GL.DeleteBuffer(_handle);
        }
    }
}
