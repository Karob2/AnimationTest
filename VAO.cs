using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;

namespace AnimationTest
{
    public class VAO : IDisposable
    {
        private uint IndexCount = 0;
        private BufferObject<byte>? Vbo;
        private BufferObject<uint>? Ebo;
        private uint Handle = 0;
        private PrimitiveType PrimitiveType;
        private bool Disposed = false;

        public VAO(VertexAttribs attribs, byte[] vertices, PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            if (Program.GL == null) return;
            PrimitiveType = primitiveType;
            IndexCount = (uint)vertices.Length / attribs.Stride;
            List<uint> indices = new List<uint>();
            for (uint i = 0; i < IndexCount; i++) indices.Add(i);
            Ebo = new BufferObject<uint>(indices.ToArray(), BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<byte>(vertices, BufferTargetARB.ArrayBuffer);
            Handle = Program.GL.GenVertexArray();
            Bind();
            Vbo.Bind();
            Ebo.Bind();
            uint attribIndex = 0;
            uint attribOffset = 0;
            uint stride = attribs.Stride;
            foreach (var a in attribs.Attributes)
            {
                VertexAttributePointer(attribIndex, a.count, a.type, stride, attribOffset);
                attribOffset += (uint)a.count * VertexAttribs.GetSizeOfType(a.type);
                attribIndex++;
            }
            Unbind();
        }

        public VAO(VertexAttribs attribs, byte[] vertices, uint[] indices, PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            if (Program.GL == null) return;
            PrimitiveType = primitiveType;
            IndexCount = (uint)indices.Length;
            Ebo = new BufferObject<uint>(indices, BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<byte>(vertices, BufferTargetARB.ArrayBuffer);
            Handle = Program.GL.GenVertexArray();
            Bind();
            Vbo.Bind();
            Ebo.Bind();
            uint attribIndex = 0;
            uint attribOffset = 0;
            uint stride = attribs.Stride;
            foreach (var a in attribs.Attributes)
            {
                VertexAttributePointer(attribIndex, a.count, a.type, stride, attribOffset);
                attribOffset += (uint)a.count * VertexAttribs.GetSizeOfType(a.type);
                attribIndex++;
            }
            Unbind();
        }

        ~VAO()
        {
            if (!Disposed) Dispose();
        }

        private unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint strideInBytes, uint offsetInBytes)
        {
            if (type == VertexAttribPointerType.Int) Program.GL?.VertexAttribIPointer(index, count, GLEnum.Int, strideInBytes, (void*)offsetInBytes);
            else Program.GL?.VertexAttribPointer(index, count, type, false, strideInBytes, (void*)offsetInBytes);

            Program.GL?.EnableVertexAttribArray(index);
        }

        private void Bind()
        {
            Program.GL?.BindVertexArray(Handle);
        }

        private void Unbind()
        {
            Program.GL?.BindVertexArray(0);
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Vbo?.Dispose();
            Ebo?.Dispose();
            //commented because it can cause an uncatchable exception
            //Program.GL?.DeleteVertexArray(Handle);
        }

        public unsafe void Draw(double delta, uint? count = null)
        {
            if (Disposed)
            {
                Console.WriteLine("Attempted to draw a disposed VAO!");
                return;
            }
            Bind();
            Program.GL?.DrawElements(PrimitiveType, count.HasValue ? count.Value : IndexCount, DrawElementsType.UnsignedInt, null);
            Unbind();
        }
    }
}
