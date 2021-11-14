using Silk.NET.OpenGL;

namespace AnimationTest
{
    public class VertexAttribs
    {
        public List<(int count, VertexAttribPointerType type)> Attributes { get; private set; } = new List<(int count, VertexAttribPointerType type)>();

        public uint Stride
        {
            get
            {
                uint s = 0;
                foreach (var a in Attributes) s += (uint)a.count * GetSizeOfType(a.type);
                return s;
            }
        }

        public void AddAttribute(int count, VertexAttribPointerType type)
        {
            Attributes.Add((count, type));
        }

        internal static uint GetSizeOfType(VertexAttribPointerType type)
        {
            switch (type)
            {
                case VertexAttribPointerType.Double: return sizeof(double);
                case VertexAttribPointerType.Int: return sizeof(int);
                case VertexAttribPointerType.Short: return sizeof(short);
                default: return sizeof(float);
            }
        }
    }
}
