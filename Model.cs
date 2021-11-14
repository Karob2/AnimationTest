using Silk.NET.OpenGL;
using System.Numerics;

namespace AnimationTest
{
    public class Model : IDisposable
    {
        private bool Initialized = false;
        private bool Disposed = false;
        private Assimp.AssimpContext? AssimpContext;
        private Assimp.Scene? Scene;

        private Dictionary<string, Node> Nodes = new Dictionary<string, Node>();
        public int NextBoneId = 0;
        private Matrix4x4 GlobalInverseTransform;

        List<uint>? Indices;
        VAO? SkeletalVAO;

        public Model(string filepath)
        {
            try
            {
                AssimpContext = new Assimp.AssimpContext();
                AssimpContext.SetConfig(new Assimp.Configs.VertexBoneWeightLimitConfig(4));
                AssimpContext.SetConfig(new Assimp.Configs.FBXStrictModeConfig(false));
                AssimpContext.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));
                AssimpContext.SetConfig(new Assimp.Configs.FBXImportCamerasConfig(false));
                AssimpContext.SetConfig(new Assimp.Configs.FavorSpeedConfig(false));
                var postProcessFlags =
                      Assimp.PostProcessSteps.LimitBoneWeights
                    | Assimp.PostProcessSteps.Triangulate
                    | Assimp.PostProcessSteps.SortByPrimitiveType
                    | Assimp.PostProcessSteps.JoinIdenticalVertices
                    | Assimp.PostProcessSteps.GenerateSmoothNormals
                    | Assimp.PostProcessSteps.FlipUVs;
                Scene = AssimpContext.ImportFile(filepath, postProcessFlags);
                Initialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            var bones = new Dictionary<string, Assimp.Bone>();
            foreach (var m in Scene.Meshes) foreach (var mb in m.Bones) bones[mb.Name] = mb;
            SearchBonesAndTransforms(0, bones, Scene.RootNode, Matrix4x4.Identity);

            var attribs = new VertexAttribs();
            attribs.AddAttribute(3, VertexAttribPointerType.Float); //bone position
            attribs.AddAttribute(1, VertexAttribPointerType.Int); //bone id

            byte[] vertexData;
            using (var memStream = new MemoryStream(NextBoneId * (int)attribs.Stride))
            {
                using (var writer = new BinaryWriter(memStream))
                {
                    foreach (var b in Nodes.Values)
                    {
                        if (b.IsBone && b.BoneIndex.HasValue)
                        {
                            writer.Write(0f);
                            writer.Write(0f);
                            writer.Write(0f);
                            writer.Write(b.BoneIndex.Value);
                        }
                    }
                    vertexData = memStream.GetBuffer();
                }
            }

            Indices = new List<uint>();
            foreach (var b in Nodes.Values)
            {
                if (b.IsBone && b.BoneIndex.HasValue && b.ParentBone != null && b.ParentBone.BoneIndex.HasValue)
                {

                    if (b.ParentBone != null)
                    {
                        Indices.Add((uint)b.BoneIndex.Value);
                        Indices.Add((uint)b.ParentBone.BoneIndex.Value);
                    }
                }
            }

            SkeletalVAO = new VAO(attribs, vertexData, Indices.ToArray(), PrimitiveType.Lines);
            Console.WriteLine("Prepared skeleton VAO with " + (Indices.Count / 2) + " lines.");
        }

        public void Draw(double delta)
        {

            if (Program.MainCamera != null)
            {
                //DRAW THE BONES
                Program.GL?.Disable(EnableCap.DepthTest);
                Program.SkeletonShader?.Use();
                Program.SkeletonShader?.SetMVP(Matrix4x4.Identity, Program.MainCamera.GetViewMatrix(), Program.MainCamera.GetProjectionMatrix());

                foreach (var b in Nodes.Values)
                {
                    if (b.IsBone && b.BoneIndex.HasValue && b.BoneOffset.HasValue)
                    {
                        Program.SkeletonShader?.SetUniform("uBones[" + b.BoneIndex + "]", GlobalInverseTransform * b.WorldSpaceTransform * b.BoneOffset.Value);
                    }
                }
                SkeletalVAO?.Draw(delta);//, 3);
                Program.GL?.UseProgram(0);
                Program.GL?.Enable(EnableCap.DepthTest);
            }
        }

        private void SearchBonesAndTransforms(int indent, Dictionary<string, Assimp.Bone> bones, Assimp.Node node, Matrix4x4 parentTransform, Node? parent = null, Node? parentBone = null)
        {
            if (!Initialized || Scene == null) return;
            bool hasBone = bones.ContainsKey(node.Name);
            Matrix4x4 thisTransform = parentTransform * ToMatrix4x4(node.Transform); //convert the mesh transform to model transform
            Node n = new Node() { Name = node.Name, Parent = parent, WorldSpaceTransform = thisTransform, IsBone = hasBone, ParentBone = parentBone };
            if (hasBone)
            {
                Assimp.Bone b = bones[node.Name];
                n.BoneOffset = ToMatrix4x4(b.OffsetMatrix);
                n.BoneIndex = NextBoneId;
                NextBoneId++;
                if (n.BoneIndex == 0)
                {
                    GlobalInverseTransform = n.WorldSpaceTransform;
                    Matrix4x4.Invert(GlobalInverseTransform, out GlobalInverseTransform);
                }
            }
            Console.WriteLine(new string(' ', indent) + "Loaded node: " + node.Name + (hasBone ? " (bone: " + n.BoneIndex + ")" : "") + " [Parent node: " + parent?.Name + "]");
            Nodes[node.Name] = n;
            parent?.Children.Add(n);
            foreach (var c in node.Children) SearchBonesAndTransforms(indent + 1, bones, c, thisTransform, n, hasBone ? n : parentBone);
        }

        private Matrix4x4 ToMatrix4x4(Assimp.Matrix4x4 matrix)
        {
            //transpose the row-major assimp matrix into a column-major System.Numerics matrix
            return new Matrix4x4(matrix.A1, matrix.B1, matrix.C1, matrix.D1,
                                 matrix.A2, matrix.B2, matrix.C2, matrix.D2,
                                 matrix.A3, matrix.B3, matrix.C3, matrix.D3,
                                 matrix.A4, matrix.B4, matrix.C4, matrix.D4);
        }

        private Quaternion ToQuaternion(Assimp.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        private Vector3 ToVector3(Assimp.Vector3D vector3d)
        {
            return new Vector3(vector3d.X, vector3d.Y, vector3d.Z);
        }

        ~Model()
        {
            if (!Disposed) Dispose();
        }

        public void Dispose()
        {
            Disposed = true;
            if (AssimpContext != null) AssimpContext.Dispose();
        }
    }
}
