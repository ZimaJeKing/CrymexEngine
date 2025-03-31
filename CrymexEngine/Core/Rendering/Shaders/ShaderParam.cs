using CrymexEngine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.Rendering
{
    public abstract class ShaderParam(string name)
    {
        public readonly string name = name;
        public int GLLocation => _glLocation;

        public static readonly ShaderParam[] defaultParams = {
                    new Vec3ShaderParam("position"),
                    new Mat4ShaderParam("transform"),
                    new Vec4ShaderParam("color")
                };

        private int _glLocation;

        public void Init(Shader reference)
        {
            _glLocation = GL.GetUniformLocation(reference._glShader, name);

            if (_glLocation < 0)
            {
                Debug.LogError($"Shader parameter '{name}' could not be found");
            }
        }
        public abstract void Set(object _value);
        protected abstract void Refresh();
    }
    public class DoubleShaderParam(string name) : ShaderParam(name)
    {
        public double value;

        public override void Set(object _value)
        {
            if (GLLocation == -1) return;

            if (_value.GetType() == typeof(float)) _value = (double)(float)_value;
            if (_value.GetType() != typeof(double))
            {
                Debug.LogError($"Wrong paramater format for '{name}'");
                return;
            }

            value = (double)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            if (GLLocation == -1) return;

            GL.Uniform1(GLLocation, value);
        }
    }
    public class Vec2ShaderParam(string name) : ShaderParam(name)
    {
        public Vector2 value;

        public override void Set(object _value)
        {
            if (GLLocation == -1) return;

            if (_value.GetType() != typeof(Vector2))
            {
                Debug.LogError($"Wrong paramater format for '{name}'");
                return;
            }

            value = (Vector2)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            if (GLLocation == -1) return;

            GL.Uniform2(GLLocation, ref value);
        }
    }
    public class Vec3ShaderParam(string name) : ShaderParam(name)
    {
        public Vector3 value;

        public override void Set(object _value)
        {
            if (GLLocation == -1) return;

            if (_value.GetType() != typeof(Vector3))
            {
                Debug.LogError($"Wrong paramater format for '{name}'");
                return;
            }

            value = (Vector3)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            if (GLLocation == -1) return;

            GL.Uniform3(GLLocation, ref value);
        }
    }
    public class Vec4ShaderParam(string name) : ShaderParam(name)
    {
        public Vector4 value;

        public override void Set(object _value)
        {
            if (GLLocation == -1) return;

            if (_value.GetType() == typeof(Color4))
            {
                Color4 col = (Color4)_value;
                value = new Vector4(col.R, col.G, col.B, col.A);
                Refresh();
                return;
            }

            if (_value.GetType() != typeof(Vector4))
            {
                Debug.LogError($"Wrong paramater format for '{name}'");
                return;
            }

            value = (Vector4)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            if (GLLocation == -1) return;

            GL.Uniform4(GLLocation, ref value);
        }
    }
    public class Mat2ShaderParam(string name) : ShaderParam(name)
    {
        public Matrix2 value;

        public override void Set(object _value)
        {
            if (GLLocation == -1) return;

            if (_value.GetType() != typeof(Matrix2))
            {
                Debug.LogError($"Wrong paramater format for '{name}'");
                return;
            }

            value = (Matrix2)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            if (GLLocation == -1) return;

            GL.UniformMatrix2(GLLocation, false, ref value);
        }
    }
    public class Mat4ShaderParam(string name) : ShaderParam(name)
    {
        public Matrix4 value;

        public override void Set(object _value)
        {
            if (GLLocation == -1) return;

            if (_value.GetType() != typeof(Matrix4))
            {
                Debug.LogError($"Wrong paramater format for '{name}'");
                return;
            }

            value = (Matrix4)_value;
            Refresh();
        }

        protected override void Refresh()
        {
            if (GLLocation == -1) return;

            GL.UniformMatrix4(GLLocation, false, ref value);
        }
    }
}
