using System.Collections.Generic;
using System.Numerics;

namespace AnimationTest
{
    public class Node
    {
        public bool IsBone;
        public int? BoneIndex;
        public Node? Parent;
        public Node? ParentBone;
        public string? Name;
        public Matrix4x4 WorldSpaceTransform;
        public Matrix4x4? BoneOffset;
        public List<Node> Children = new List<Node>();
    }
}
