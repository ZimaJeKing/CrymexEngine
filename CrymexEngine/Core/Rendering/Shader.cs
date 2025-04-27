using OpenTK.Graphics.OpenGL;

namespace CrymexEngine.Rendering
{
    public class Shader
    {
        public static Shader Regular
        {
            get => _regular;
            set
            {
                if (value != null) _regular = value;
            }
        }
        public static Shader UI
        {
            get => _ui;
            set
            {
                if (value != null) _ui = value;
            }
        }

        public readonly int _glShader = 0;
        public readonly ShaderParam[] parameters;

        private static Shader _regular;
        private static Shader _ui;
        private static bool _defaultShadersLoaded = false;

        public static void LoadDefaultShaders()
        {
            if (_defaultShadersLoaded) return;

            string? regularShaderName = null, uiShaderName = null;

            if (Settings.GlobalSettings.GetSetting("DefaultEntityShader", out SettingOption regularShaderOption, SettingType.RefString))
            {
                regularShaderName = regularShaderOption.GetValue<string>();
            }
            if (Settings.GlobalSettings.GetSetting("DefaultUIShader", out SettingOption uiShaderOption, SettingType.RefString))
            {
                uiShaderName = uiShaderOption.GetValue<string>();
            }

            if (regularShaderName != null) _regular = Assets.GetShaderBroad(regularShaderName);
            if (_regular == null) _regular = LoadFromAsset(DefaultShaders.regularVertex, DefaultShaders.regularFragment);

            if (uiShaderName != null) _ui = Assets.GetShaderBroad(uiShaderName);
            if (_ui == null) _ui = LoadFromAsset(DefaultShaders.uiVertex, DefaultShaders.uiFragment);

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

        private static class DefaultShaders
        {
            public static readonly string regularVertex = "#version 450 core\r\n#usedefaultparams\r\n\r\nlayout (location = 0) in vec2 aPosition;\r\nlayout (location = 1) in vec2 aTexCoord;\r\n\r\nout vec2 texCoord;\r\n\r\nuniform vec3 position;\r\nuniform mat4 transform;\r\n\r\nvoid main()\r\n{\r\n    gl_Position = vec4(aPosition, 0, 1.0) * transform;\r\n    gl_Position.xyz += position;\r\n\r\n    texCoord.xy = aTexCoord;\r\n}";
            public static readonly string regularFragment = "#version 450 core\r\n\r\nin vec2 texCoord;\r\n\r\nuniform sampler2D texture0;\r\nuniform vec4 color;\r\n\r\nout vec4 FragColor;\r\n\r\nvoid main()\r\n{\r\n    vec4 texColor;\r\n    texColor = texture(texture0, texCoord);\r\n    \r\n    texColor *= color;\r\n\r\n    if (texColor.a < 0.5f) discard;\r\n\r\n    FragColor = texColor;\r\n}";

            public static readonly string uiVertex = "#version 450 core\r\n#usedefaultparams\r\n\r\nlayout(location = 0) in vec2 aPosition;\r\nlayout (location = 1) in vec2 aTexCoord;\r\n\r\nout vec2 texCoord;\r\n\r\nuniform vec3 position;\r\nuniform mat4 transform;\r\n\r\nvoid main()\r\n{\r\n    gl_Position = vec4(aPosition, 0.0, 1.0) * transform;\r\n    gl_Position.xyz += position;\r\n\r\n    texCoord.xy = aTexCoord;\r\n}";
            public static readonly string uiFragment = "#version 450 core\r\n#shaderparam vec4 uvTransform\r\n\r\nin vec2 texCoord;\r\n\r\nuniform sampler2D texture0;\r\nuniform vec4 color;\r\nuniform vec4 uvTransform;\r\n\r\nout vec4 FragColor;\r\n\r\nvoid main()\r\n{\r\n    vec4 texColor;\r\n    texColor = texture(texture0, (texCoord * uvTransform.rg) + uvTransform.ba);\r\n    \r\n    texColor *= color;\r\n\r\n    FragColor = texColor;\r\n}";
        }
    }
}