using System.Numerics;

namespace AnimationTest
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 UV;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;
        public (int X, int Y, int Z, int W) BoneIds;
        public Vector4 BoneWeights;
    }
}
