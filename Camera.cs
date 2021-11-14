using System;
using System.Numerics;

namespace AnimationTest
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Front { get; set; }
        public Vector3 Up { get; private set; }
        public float AspectRatio { get; set; }
        public float Yaw { get; set; } = -90f;
        public float Pitch { get; set; }

        public Camera(Vector3 position, Vector3 front, Vector3 up, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
            Front = front;
            Up = up;
        }

        public void ModifyDirection(float xOffset, float yOffset)
        {
            Yaw -= xOffset;
            Pitch += yOffset;
            Pitch = Math.Clamp(Pitch, -89f, 89f);

            var cameraDirection = Vector3.Zero;
            cameraDirection.X = MathF.Cos(DegreesToRadians(Yaw)) * MathF.Cos(DegreesToRadians(Pitch));
            cameraDirection.Y = MathF.Sin(DegreesToRadians(Pitch));
            cameraDirection.Z = MathF.Sin(DegreesToRadians(Yaw)) * MathF.Cos(DegreesToRadians(Pitch));

            Front = Vector3.Normalize(cameraDirection);
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(Position, Position + Front, Up);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(DegreesToRadians(45f), AspectRatio, 0.1f, 5000.0f);
        }

        public float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }
    }
}
