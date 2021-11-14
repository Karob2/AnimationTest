using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;

namespace AnimationTest
{
    public class Shader
    {
        public string Name { get; private set; }
        private uint Handle;
        private Dictionary<string, uint?>? UniformLocations;
        public Shader(string filepath)
        {
            if (!File.Exists(filepath))
            {
                string message = "Shader cannot be loaded. File does not exist: " + filepath;
                Console.WriteLine(message);
                throw new Exception(message);
            }
            Name = Path.GetFileName(filepath);
            string shaderSource = File.ReadAllText(filepath);
            string vsrc = Preprocess(ShaderType.VertexShader, shaderSource);
            string fsrc = Preprocess(ShaderType.FragmentShader, shaderSource);
            Handle = CreateProgram(vsrc, fsrc);
            Console.WriteLine("Prepared shader: " + Name);
        }

        private string Preprocess(ShaderType type, string shaderSource)
        {
            string? shaderDefine = null;
            if (type == ShaderType.VertexShader) shaderDefine = "#define C_VERTEX true";
            else if (type == ShaderType.FragmentShader) shaderDefine = "#define C_FRAGMENT true";
            if (shaderDefine != null) shaderSource = shaderSource.Replace("//$$.C", shaderDefine);
            return shaderSource;
        }

        public void Use()
        {
            Program.GL?.UseProgram(Handle);
        }

        public static uint CreateProgram(string vsrc, string fsrc)
        {
            if (Program.GL == null) return 0;
            uint program = Program.GL.CreateProgram();

            uint vertex = LoadShader(ShaderType.VertexShader, vsrc);
            uint fragment = LoadShader(ShaderType.FragmentShader, fsrc);
            Program.GL.AttachShader(program, vertex);
            Program.GL.AttachShader(program, fragment);
            Program.GL.LinkProgram(program);
            Program.GL.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                string message = $"Program failed to link with error: {Program.GL.GetProgramInfoLog(program)}";
                Console.WriteLine(message);
                throw new Exception(message);
            }
            Program.GL.DetachShader(program, vertex);
            Program.GL.DetachShader(program, fragment);
            Program.GL.DeleteShader(vertex);
            Program.GL.DeleteShader(fragment);

            return program;
        }

        private static uint LoadShader(ShaderType type, string source)
        {
            if (Program.GL == null) return 0;
            uint handle = Program.GL.CreateShader(type);
            Program.GL.ShaderSource(handle, source);
            Program.GL.CompileShader(handle);
            string infoLog = Program.GL.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                string message = $"Error compiling {type} shader: {infoLog}";
                Console.WriteLine(message);
                throw new Exception(message);
            }
            return handle;
        }

        private uint? GetUniformLocation(string name)
        {
            if (UniformLocations == null) UniformLocations = new Dictionary<string, uint?>();
            if (UniformLocations.ContainsKey(name)) return UniformLocations[name];
            if (Program.GL != null)
            {
                int v = Program.GL.GetUniformLocation(Handle, name);
                if (v >= 0) UniformLocations[name] = (uint)v;
                else UniformLocations[name] = null;
            }
            if (UniformLocations[name] == null) Console.WriteLine("Uniform not found on shader " + Name + ": " + name);
            return UniformLocations[name];
        }

        public void SetUniform(string location, int value)
        {
            uint? key = GetUniformLocation(location);
            if (key.HasValue) SetUniform(key.Value, value);
        }
        private void SetUniform(uint location, int value)
        {
            Program.GL?.Uniform1((int)location, value);
        }

        public void SetUniform(string location, Matrix4x4 value)
        {
            uint? key = GetUniformLocation(location);
            if (key.HasValue) SetUniform(key.Value, value);
        }
        private unsafe void SetUniform(uint location, Matrix4x4 value)
        {
            Program.GL?.UniformMatrix4((int)location, 1, false, (float*)&value);
        }

        public void SetUniform(string location, Matrix4x4[] value)
        {
            uint? key = GetUniformLocation(location);
            if (key.HasValue) SetUniform(key.Value, value);
        }

        private unsafe void SetUniform(uint location, Matrix4x4[] value)
        {
            if (Program.GL == null) return;
            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            IntPtr address = handle.AddrOfPinnedObject();
            Program.GL.UniformMatrix4((int)location, (uint)value.Length, false, (float*)address.ToPointer());
            handle.Free();
        }

        public void SetUniform(string location, float value)
        {
            uint? key = GetUniformLocation(location);
            if (key.HasValue) SetUniform(key.Value, value);
        }
        private void SetUniform(uint location, float value)
        {
            Program.GL?.Uniform1((int)location, value);
        }

        public void SetUniform(string location, Vector2 value)
        {
            uint? key = GetUniformLocation(location);
            if (key.HasValue) SetUniform(key.Value, value);
        }
        private void SetUniform(uint location, Vector2 value)
        {
            Program.GL?.Uniform2((int)location, value);
        }

        public void SetUniform(string location, Vector3 value)
        {
            uint? key = GetUniformLocation(location);
            if (key.HasValue) SetUniform(key.Value, value);
        }
        private void SetUniform(uint location, Vector3 value)
        {
            Program.GL?.Uniform3((int)location, value);
        }

        public void SetUniform(string location, Vector4 value)
        {
            uint? key = GetUniformLocation(location);
            if (key.HasValue) SetUniform(key.Value, value);
        }
        private void SetUniform(uint location, Vector4 value)
        {
            Program.GL?.Uniform4((int)location, value);
        }
    }
}
