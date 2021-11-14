using System.Numerics;

namespace AnimationTest
{
    public class SkeletonShader : Shader
    {
        public SkeletonShader() : base("Skeleton.glsl")
        {
            Use();
            Matrix4x4[] identities = new Matrix4x4[120];
            for (int i = 0; i < 120; i++)
            {
                identities[i] = Matrix4x4.Identity;
            }
            SetUniform("uBones", identities);
        }

        public void SetMVP(Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
        {
            SetUniform("uModel", model);
            SetUniform("uView", view);
            SetUniform("uProjection", projection);
        }
    }
}
