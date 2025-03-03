using FreeTypeSharp;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.Rendering
{
    public class Shader
    {
        public static Shader Regular
        {
            get
            {
                return _regular;
            }
            set
            {
                if (value != null) _regular = value;
            }
        }
        public static Shader UI
        {
            get
            {
                return _ui;
            }
            set
            {
                if (value != null) _ui = value;
            }
        }

        public static Shader Line
        {
            get
            {
                return _line;
            }
            set
            {
                if (value != null) _line = value;
            }
        }

        public readonly int _glShader = 0;
        public readonly ShaderParam[] parameters;

        private static Shader _regular;
        private static Shader _ui;
        private static Shader _line;
        private static bool _defaultShadersLoaded = false;

        public static void LoadDefaultShaders()
        {
            if (_defaultShadersLoaded) return;

            string regularShaderName = "Regular";
            string uiShaderName = "UI";
            string lineShaderName = "Line";

            if (Settings.GlobalSettings.GetSetting("DefaultEntityShader", out SettingOption regularShaderOption, SettingType.RefString))
            {
                regularShaderName = regularShaderOption.GetValue<string>();
            }
            if (Settings.GlobalSettings.GetSetting("DefaultUIShader", out SettingOption uiShaderOption, SettingType.RefString))
            {
                uiShaderName = uiShaderOption.GetValue<string>();
            }
            if (Settings.GlobalSettings.GetSetting("DefaultLineShader", out SettingOption lineShaderOption, SettingType.RefString))
            {
                lineShaderName = lineShaderOption.GetValue<string>();
            }

            _regular = Assets.GetShaderBroad(regularShaderName);
            _ui = Assets.GetShaderBroad(uiShaderName);
            _line = Assets.GetShaderBroad(lineShaderName);

            if (_regular == null || _ui == null || _line == null)
            {
                Debug.LogWarning("The default shaders couldn't be loaded properly");
                return;
            }

            _defaultShadersLoaded = true;
        }

        public Shader(string vertexCode, string fragmentCode, ShaderParam[]? parameters = null, bool hasDefaultParams = false)
        {
            if (hasDefaultParams)
            {
                if (parameters == null) parameters = Array.Empty<ShaderParam>();

                this.parameters = new ShaderParam[parameters.Length + 3];
                ShaderParam.defaultParams.CopyTo(this.parameters, 0);
                parameters.CopyTo(this.parameters, 3);
            }
            else if (parameters == null)
            {
                this.parameters = Array.Empty<ShaderParam>();
            }
            else this.parameters = parameters;

            // Compile shaders and link program
            int vertexShader = CompileShader(vertexCode, ShaderType.VertexShader);
            int fragmentShader = CompileShader(fragmentCode, ShaderType.FragmentShader);

            _glShader = GL.CreateProgram();
            GL.AttachShader(_glShader, vertexShader);
            GL.AttachShader(_glShader, fragmentShader);
            GL.LinkProgram(_glShader);

            // Check for linking errors
            GL.GetProgram(_glShader, GetProgramParameterName.LinkStatus, out var success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(_glShader);
                Debug.LogError($"Error linking shader program:\n{infoLog}");
            }

            // Clean up shaders
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            for (int p = 0; p < this.parameters.Length; p++)
            {
                this.parameters[p].Init(this);
            }
        }

        public static Shader LoadFromAsset(string rawVertexCode, string rawFragmentCode)
        {
            ShaderParam[]? parameters = LoadParameters(rawVertexCode, rawFragmentCode, out string vertexCode, out string fragmentCode, out bool hasDefaultParams);

            return new Shader(vertexCode, fragmentCode, parameters, hasDefaultParams);
        }

        public void Use()
        {
            GL.UseProgram(_glShader);
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
            if (param < 0 || param >= parameters.Length)
            {
                Debug.LogError($"Shader: Wrong parameter index \"{param}\"");
                return;
            }

            parameters[param].Set(value);
        }

        /// <returns>-1 if not found</returns>
        public int GetParamIndex(string name)
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
                Debug.LogError("Shader compilation:\n" + infoLog);
            }

            return shaderId;
        }

        private static ShaderParam[]? LoadParameters(string assetVertexCode, string assetFragmentCode, out string newVertexText, out string newFragmentText, out bool hasDefaultParams)
        {
            List<ShaderParam> paramsList = new List<ShaderParam>();

            newVertexText = "";
            newFragmentText = "";

            hasDefaultParams = false;

            // Vertex
            string[] vertexLines = assetVertexCode.Split('\n', StringSplitOptions.TrimEntries);
            for (int i = 0; i < vertexLines.Length; i++)
            {
                string line = vertexLines[i];
                if (string.IsNullOrEmpty(line)) continue;

                if (line.Length > 12 && line[0] == '#' && line.Substring(1, 11) == "shaderparam")
                {
                    string[] words = line.Split(' ', StringSplitOptions.TrimEntries);

                    if (words.Length < 3) continue;

                    ShaderParam? parameter = ParamFromString(words[1], words[2]);

                    if (parameter != null)
                    {
                        paramsList.Add(parameter);
                        continue;
                    }

                    Debug.LogError($"Shader parameter could not be loaded: {line}");
                    continue;
                }
                if (line.Length > 16 && line[0] == '#' && line.Substring(1) == "usedefaultparams")
                {
                    hasDefaultParams = true;
                    continue;
                }
                newVertexText += line + '\n';
            }

            // Fragment
            string[] fragmentLines = assetFragmentCode.Split('\n', StringSplitOptions.TrimEntries);
            for (int i = 0; i < fragmentLines.Length; i++)
            {
                string line = fragmentLines[i];
                if (string.IsNullOrEmpty(line)) continue;

                if (line.Length > 12 && line[0] == '#' && line.Substring(1, 11) == "shaderparam")
                {
                    string[] words = line.Split(' ', StringSplitOptions.TrimEntries);

                    if (words.Length < 3) continue;

                    ShaderParam? parameter = ParamFromString(words[1], words[2]);

                    if (parameter != null)
                    {
                        paramsList.Add(parameter);
                        continue;
                    }
                }
                if (line.Length > 16 && line[0] == '#' && line.Substring(1) == "usedefaultparams")
                {
                    hasDefaultParams = true;
                    continue;
                }
                newFragmentText += line + '\n';
            }

            if (paramsList.Count == 0) return null;
            return paramsList.ToArray();
        }

        private static ShaderParam? ParamFromString(string type, string name)
        {
            switch (type)
            {
                case "double":
                    {
                        return new DoubleShaderParam(name);
                    }
                case "vec2":
                    {
                        return new Vec2ShaderParam(name);
                    }
                case "vec3":
                    {
                        return new Vec3ShaderParam(name);
                    }
                case "vec4":
                    {
                        return new Vec4ShaderParam(name);
                    }
                case "mat2":
                    {
                        return new Mat2ShaderParam(name);
                    }
                case "mat4":
                    {
                        return new Mat4ShaderParam(name);
                    }
            }

            return null;
        }
    }

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