using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;

namespace AnimationTest
{
    public static class Program
    {
        private static IWindow? SilkWindow = null;
        public static Silk.NET.OpenGL.GL? GL { get; private set; } = null;
        private static IInputContext? SilkInput = null;

        public static Camera? MainCamera;
        private static int Width, Height;

        private static Model? Model;

        private static bool CursorLocked = false;
        private static Vector2 LastMousePosition;
        private static Silk.NET.Maths.Vector2D<float> MouseLookVector;
        private static bool LastCursorLockPressedState = false;

        public static SkeletonShader? SkeletonShader;

        public static void Main()
        {
            var options = WindowOptions.Default;
            Width = 1280;
            Height = 800;
            options.Size = new Silk.NET.Maths.Vector2D<int>(Width, Height);
            options.API = GraphicsAPI.Default;
            options.VSync = true;
            options.Title = "Animation Test";
            SilkWindow = Window.Create(options);
            SilkWindow.Load += OnLoad;
            SilkWindow.Update += OnUpdate;
            SilkWindow.Render += OnRender;
            SilkWindow.Resize += ns => { 
                GL?.Viewport(ns);
                Width = (int)ns.X;
                Height = (int)ns.Y;
            };
            SilkWindow.Run();
        }

        public static Matrix4x4 GetUIProjectionMatrix()
        {
            return Matrix4x4.CreateOrthographicOffCenter(0, Width, Height, 0, 0f, 100f);
        }

        public static void ToggleCursorLock()
        {
            if (CursorLocked) UnlockCursor();
            else LockCursor();
        }

        public static void LockCursor()
        {
            if (SilkInput == null) return;
            CursorLocked = true;
            foreach (var m in SilkInput.Mice)
            {
                m.Cursor.CursorMode = CursorMode.Raw;
                m.MouseMove += OnMouseMove;
            }
        }

        public static void UnlockCursor()
        {
            if (SilkInput == null) return;
            CursorLocked = false;
            foreach (var m in SilkInput.Mice)
            {
                m.Cursor.CursorMode = CursorMode.Normal;
                m.MouseMove -= OnMouseMove;
            }
        }

        private static void OnMouseMove(IMouse mouse, Vector2 position)
        {
            const float lookSensitivity = 0.1f;
            if (LastMousePosition == default) { LastMousePosition = position; }
            else
            {
                var xOffset = -((position.X - LastMousePosition.X) * lookSensitivity);
                var yOffset = -((position.Y - LastMousePosition.Y) * lookSensitivity);
                LastMousePosition = position;

                MouseLookVector = new Silk.NET.Maths.Vector2D<float>(xOffset, yOffset);
            }
        }

        public static Silk.NET.Maths.Vector2D<float> GetMouseLookVector()
        {
            if (!CursorLocked) return Silk.NET.Maths.Vector2D<float>.Zero;
            var mlv = MouseLookVector;
            return mlv;
        }

        private static void OnUpdate(double delta)
        {
            if (SilkWindow == null || GL == null || MainCamera == null || SilkInput == null) return;

            if (SilkInput.Keyboards.First().IsKeyPressed(Key.Escape))
            {
                if(!LastCursorLockPressedState) ToggleCursorLock();
                LastCursorLockPressedState = true;
            }
            else LastCursorLockPressedState = false;

            var moveSpeed = 25f * (float)delta;

            if (SilkInput.Keyboards.First().IsKeyPressed(Key.W))
            {
                MainCamera.Position += moveSpeed * MainCamera.Front;
            }
            if (SilkInput.Keyboards.First().IsKeyPressed(Key.S))
            {
                MainCamera.Position -= moveSpeed * MainCamera.Front;
            }
            if (SilkInput.Keyboards.First().IsKeyPressed(Key.A))
            {
                MainCamera.Position -= Vector3.Normalize(Vector3.Cross(MainCamera.Front, MainCamera.Up)) * moveSpeed;
            }
            if (SilkInput.Keyboards.First().IsKeyPressed(Key.D))
            {
                MainCamera.Position += Vector3.Normalize(Vector3.Cross(MainCamera.Front, MainCamera.Up)) * moveSpeed;
            }

            var mlv = GetMouseLookVector();
            MainCamera.ModifyDirection(mlv.X, mlv.Y);
            MouseLookVector = Silk.NET.Maths.Vector2D<float>.Zero;
        }

        private static void OnRender(double delta)
        {
            if (SilkWindow == null || GL == null) return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Model?.Draw(delta);
        }

        public static void OnLoad()
        {
            GL = GL.GetApi(SilkWindow);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(System.Drawing.Color.FromArgb(255, 31, 24, 37));
            if (SilkWindow != null) SilkInput = SilkWindow.CreateInput();
            SilkWindow?.Center();
            GL.Viewport(new System.Drawing.Size(Width, Height));
            MainCamera = new Camera(Vector3.UnitZ * 25, Vector3.UnitZ * -1, Vector3.UnitY, (float)Width / Height);
            SkeletonShader = new SkeletonShader();
            Model = new Model("BaseMesh_Anim.fbx");
            LockCursor();
        }
    }
}