using Silk.NET.OpenGL;
using System;

namespace AnimationTest
{
    public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
    {
        private uint Handle;
        private BufferTargetARB BufferType;
        private bool Disposed = false;

        public unsafe BufferObject(Span<TDataType> data, BufferTargetARB bufferType)
        {
            if (Program.GL == null) return;
            BufferType = bufferType;
            Handle = Program.GL.GenBuffer();
            Bind();
            fixed (void* d = data) Program.GL.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }

        ~BufferObject()
        {
            Dispose();
        }

        public void Bind()
        {
            Program.GL?.BindBuffer(BufferType, Handle);
        }

        public void Unbind()
        {
            Program.GL?.BindBuffer(BufferType, 0);
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Program.GL?.DeleteBuffer(Handle);
        }
    }
}
