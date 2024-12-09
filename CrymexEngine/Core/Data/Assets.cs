using CrymexEngine.Data;
using CrymexEngine.Rendering;
using OpenTK.Graphics.OpenGL;

namespace CrymexEngine
{
    public static class Assets
    {
        public static bool Precompiled
        {
            get
            {
                return _precompiled;
            }
        }

        private static List<TextureAsset> _textureAssets = new();
        private static List<AudioAsset> _audioAssets = new();
        private static List<DataAsset> _scenes = new();
        private static List<ShaderAsset> _shaderAssets = new();

        private static bool _precompiled;

        public static void LoadAssets()
        {
            // Defining Texture.White and Texture.None
            Texture.White = new Texture(1, 1, new byte[] { 255, 255, 255, 255 });

            Texture.None = new Texture(1, 1, new byte[] { 0, 0, 0, 0 });

            _precompiled = Settings.GetSetting("LoadPrecompiled", out SettingOption usePrecompiledAssetsOption, SettingType.Bool) && usePrecompiledAssetsOption.GetValue<bool>();
            if (Precompiled)
            {
                _precompiled = true;

                // Loads only precompiled assets
                LoadPrecompiledAssets();
            }
            else
            {
                _precompiled = false;

                // Starts a recursive loop of searching directories in the "Assets" folder
                // Responsible for loading all non-precompiled assets
                SearchDirectory(Debug.assetsPath);

                // Remove incomplete shaders


                // Precompile assets if specified
                if (Settings.GetSetting("PrecompileAssets", out SettingOption precompileAssetsOption, SettingType.Bool) && precompileAssetsOption.GetValue<bool>())
                {
                    CompileDataAssets();
                }
            }

            // Set the missing texture
            Texture.Missing = GetTexture("Missing");
            if (Texture.Missing == null) Texture.Missing = Texture.None;

            Shader.LoadDefaultShaders();
        }

        public static Texture GetTexture(string name)
        {
            foreach (TextureAsset texture in _textureAssets)
            {
                if (texture.name == name)
                {
                    return texture.texture;
                }
            }
            return Texture.Missing;
        }
        public static string GetScenePath(string name)
        {
            foreach (DataAsset asset in _scenes)
            {
                if (asset.name == name)
                {
                    return asset.path;
                }
            }
            return "";
        }
        public static AudioClip GetAudioClip(string name)
        {
            foreach (AudioAsset asset in _audioAssets)
            {
                if (asset.name == name)
                {
                    return asset.clip;
                }
            }
            return null;
        }
        public static Shader GetShader(string name)
        {
            foreach (ShaderAsset asset in _shaderAssets)
            {
                if (asset.name == name)
                {
                    return asset.shader;
                }
            }
            return null;
        }
        public static void Cleanup()
        {
            foreach (AudioAsset asset in _audioAssets)
            {
                asset.clip.Dispose();
            }
            foreach (TextureAsset asset in _textureAssets)
            {
                asset.texture.Dispose();
            }
        }

        private static void SearchDirectory(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);

            for (int i = 0; i < files.Length; i++)
            {
                LoadAsset(files[i]);
            }
            for (int i = 0; i < directories.Length; i++)
            {
                SearchDirectory(directories[i]);
            }
        }

        private static void LoadAsset(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            string fileExtension = Path.GetExtension(path).ToLower();

            switch (fileExtension.ToLower())
            {
                case ".png":
                    {
                        _textureAssets.Add(new TextureAsset(path, Texture.Load(path)));
                        break;
                    }
                case ".jpg":
                    {
                        _textureAssets.Add(new TextureAsset(path, Texture.Load(path)));
                        break;
                    }
                case ".jpeg":
                    {
                        _textureAssets.Add(new TextureAsset(path, Texture.Load(path)));
                        break;
                    }
                case ".bmp":
                    {
                        _textureAssets.Add(new TextureAsset(path, Texture.Load(path)));
                        break;
                    }
                case ".gif":
                    {
                        _textureAssets.Add(new TextureAsset(path, Texture.Load(path)));
                        break;
                    }
                case ".wav":
                    {
                        AudioClip? clip = AudioClip.Load(path);
                        if (clip == null) break;
                        _audioAssets.Add(new AudioAsset(path, clip));
                        break;
                    }
                case ".mp3":
                    {
                        AudioClip? clip = AudioClip.Load(path);
                        if (clip == null) break;
                        _audioAssets.Add(new AudioAsset(path, clip)); 
                        break;
                    }
                case ".scene":
                    {
                        _scenes.Add(new DataAsset(path));
                        break;
                    }
                case ".vertex":
                    {
                        string? fragmentPath = Path.GetDirectoryName(path) + '\\' + filename + ".fragment";
                        if (fragmentPath != null && File.Exists(fragmentPath))
                        {
                            string vertexCode = File.ReadAllText(path);
                            string fragmentCode = File.ReadAllText(fragmentPath);
                            _shaderAssets.Add(new ShaderAsset(path, Shader.LoadFromAsset(vertexCode, fragmentCode), vertexCode, fragmentCode));
                        }
                        break;
                    }
            }
        }

        private static void CompileDataAssets()
        {
            // Compiling texture data
            string texturePath = Debug.runtimeAssetsPath + "RuntimeTextures.rtmAsset";
            FileStream textureFileStream = File.Create(texturePath);

            foreach (TextureAsset asset in _textureAssets)
            {
                textureFileStream.Write(AssetCompiler.CompileTextureAsset(asset));
            }
            textureFileStream.Close();

            // Compiling audio data
            string audioPath = Debug.runtimeAssetsPath + "RuntimeAudioClips.rtmAsset";
            FileStream audioFileStream = File.Create(audioPath);

            foreach (AudioAsset asset in _audioAssets)
            {
                audioFileStream.Write(AssetCompiler.CompileAudioAsset(asset));
            }
            audioFileStream.Close();

            // Compiling shaders
            string shaderPath = Debug.runtimeAssetsPath + "RuntimeShaders.rtmAsset";
            FileStream shaderFileStream = File.Create(shaderPath);

            foreach (ShaderAsset asset in _shaderAssets)
            {
                shaderFileStream.Write(AssetCompiler.CompileShaderAsset(asset));
            }
            shaderFileStream.Close();
        }

        private static void LoadPrecompiledAssets()
        {
            string texturePath = Debug.runtimeAssetsPath + "RuntimeTextures.rtmAsset";
            if (File.Exists(texturePath)) _textureAssets = AssetCompiler.DecompileTextureAssets(File.ReadAllBytes(texturePath));

            string audioPath = Debug.runtimeAssetsPath + "RuntimeAudioClips.rtmAsset";
            if (File.Exists(audioPath)) _audioAssets = AssetCompiler.DecompileAudioAssets(File.ReadAllBytes(audioPath));

            string shaderPath = Debug.runtimeAssetsPath + "RuntimeShaders.rtmAsset";
            if (File.Exists(shaderPath)) _shaderAssets = AssetCompiler.DecompileShaderAssets(File.ReadAllBytes(shaderPath));
        }
    }

    public class TextureAsset : DataAsset
    {
        public readonly Texture texture;

        public TextureAsset(string path, Texture texture) : base(path)
        {
            this.texture = texture;
        }
    }

    public class AudioAsset : DataAsset
    {
        public readonly AudioClip clip;

        public AudioAsset(string path, AudioClip clip) : base(path)
        {
            this.clip = clip;
        }
    }

    public class ShaderAsset : DataAsset
    {
        public readonly Shader shader;
        public readonly string vertexAssetCode;
        public readonly string fragmentAssetCode;

        public ShaderAsset(string path, Shader shader, string vertexAssetCode, string fragmentAssetCode) : base(path)
        {
            this.shader = shader;
            this.vertexAssetCode = vertexAssetCode;
            this.fragmentAssetCode = fragmentAssetCode;
        }
    }
}
