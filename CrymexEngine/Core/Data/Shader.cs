using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.Rendering
{
    public class Shader
    {
        public static Shader regular;
        public static Shader postProcessing;

        public readonly int _glShader;
        public ShaderParam[] parameters;

        public static void LoadDefaultShaders()
        {
            regular = new Shader(
                Debug.assetsPath + "Shaders\\RegularVertex.glsl",
                Debug.assetsPath + "Shaders\\RegularFragment.glsl",
                null
                );
        }

        public Shader(string vertexShaderPath, string fragmentShaderPath, ShaderParam[]? parameters)
        {
            if (parameters == null) parameters = new ShaderParam[0];
            if (!File.Exists(vertexShaderPath) || !File.Exists(fragmentShaderPath))
            {
                Debug.LogL("One or more shaders not found\n" + vertexShaderPath + '\n' + fragmentShaderPath, ConsoleColor.DarkRed);
                return;
            }

            this.parameters = new ShaderParam[parameters.Length + 3];
            ShaderParam.defaultParams.CopyTo(this.parameters, 0);
            parameters.CopyTo(this.parameters, 3);
            
            string vertexShaderSource = File.ReadAllText(vertexShaderPath);
            string fragmentShaderSource = File.ReadAllText(fragmentShaderPath);

            // Compile shaders and link program
            int vertexShader = CompileShader(vertexShaderSource, ShaderType.VertexShader);
            int fragmentShader = CompileShader(fragmentShaderSource, ShaderType.FragmentShader);

            _glShader = GL.CreateProgram();
            GL.AttachShader(_glShader, vertexShader);
            GL.AttachShader(_glShader, fragmentShader);
            GL.LinkProgram(_glShader);

            // Check for linking errors
            GL.GetProgram(_glShader, GetProgramParameterName.LinkStatus, out var success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(_glShader);
                throw new Exception($"Error linking program: {infoLog}");
            }

            // Clean up shaders
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            for (int p = 0; p < this.parameters.Length; p++)
            {
                this.parameters[p].Init(this);
            }
        }

        public void Use()
        {
            GL.UseProgram(_glShader);
        }

        private static int CompileShader(string source, ShaderType type)
        {
            int shaderId = GL.CreateShader(type);
            GL.ShaderSource(shaderId, source);
            GL.CompileShader(shaderId);

            // Check for compilation errors
            GL.GetShader(shaderId, ShaderParameter.CompileStatus, out var success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shaderId);
                throw new Exception($"Error compiling {type} shader: {infoLog}");
            }

            return shaderId;
        }

        public void SetParam(string name, object value)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == name)
                {
                    parameters[i].Set(value);
                    break;
                }
            }
        }
        public void SetParam(int param, object value)
        {
            if (param < 0 || param >= parameters.Length) return;

            parameters[param].Set(value);
        }

        /// <returns>-1 if not found</returns>
        public int GetParamLocation(string name)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        // Cleanup resources when the program is no longer needed
        public void Dispose()
        {
            GL.DeleteProgram(_glShader);
        }
    }

    public abstract class ShaderParam
    {
        public string name { get; private set; }
        public int location { get; private set; }
        private Shader reference;

        public static ShaderParam[] defaultParams = {
                    new Vec2ShaderParam("position"),
                    new Mat4ShaderParam("transform"),
                    new Vec4ShaderParam("color")
                };

        public ShaderParam(string name)
        {
            this.name = name;
        }

        public void Init(Shader reference)
        {
            this.reference = reference;
            location = GL.GetUniformLocation(reference._glShader, name);
        }
        public abstract void Set(object _value);
        protected abstract void Refresh();
    }
    public class DoubleShaderParam : ShaderParam
    {
        public double value;

        public DoubleShaderParam(string name) : base(name)
        {

        }

        public override void Set(object _value)
        {
            if (_value.GetType() != typeof(double) && _value.GetType() != typeof(float)) return;

            value = (double)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            GL.Uniform1(location, value);
        }
    }
    public class Vec2ShaderParam : ShaderParam
    {
        public Vector2 value;

        public Vec2ShaderParam(string name) : base(name)
        {

        }

        public override void Set(object _value)
        {
            if (_value.GetType() != typeof(Vector2)) return;

            value = (Vector2)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            GL.Uniform2(location, ref value);
        }
    }
    public class Vec3ShaderParam : ShaderParam
    {
        public Vector3 value;

        public Vec3ShaderParam(string name) : base(name)
        {

        }

        public override void Set(object _value)
        {
            if (_value.GetType() != typeof(Vector3)) return;

            value = (Vector3)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            GL.Uniform3(location, ref value);
        }
    }
    public class Vec4ShaderParam : ShaderParam
    {
        public Vector4 value;

        public Vec4ShaderParam(string name) : base(name)
        {

        }

        public override void Set(object _value)
        {
            if (_value.GetType() == typeof(Color4))
            {
                Color4 col = (Color4)_value;
                value = new Vector4(col.R, col.G, col.B, col.A);
                Refresh();
                return;
            }

            if (_value.GetType() != typeof(Vector4)) return;

            value = (Vector4)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            GL.Uniform4(location, ref value);
        }
    }
    public class Mat2ShaderParam : ShaderParam
    {
        public Matrix2 value;

        public Mat2ShaderParam(string name) : base(name)
        {

        }

        public override void Set(object _value)
        {
            if (_value.GetType() != typeof(Matrix2)) return;

            value = (Matrix2)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            GL.UniformMatrix2(location, false, ref value);
        }
    }
    public class Mat4ShaderParam : ShaderParam
    {
        public Matrix4 value;

        public Mat4ShaderParam(string name) : base(name)
        {

        }

        public override void Set(object _value)
        {
            if (_value.GetType() != typeof(Matrix4)) return;

            value = (Matrix4)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            GL.UniformMatrix4(location, false, ref value);
        }
    }
}