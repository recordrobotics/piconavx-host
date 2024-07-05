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

        public unsafe BufferObject(Span<TDataType> data, BufferTargetARB bufferType)
        {
            _bufferType = bufferType;

            _handle = Window.GL.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                Window.GL.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
        }

        public void Bind()
        {
            //Binding the buffer object, with the correct buffer type.
            Window.GL.BindBuffer(_bufferType, _handle);
        }

        public void Dispose()
        {
            //Remember to delete our buffer.
            Window.GL.DeleteBuffer(_handle);
        }
    }
}
