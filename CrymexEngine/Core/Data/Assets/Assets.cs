using CrymexEngine.Data;
using CrymexEngine.Debugging;
using CrymexEngine.Rendering;
using System.Text;

namespace CrymexEngine
{
    public class Assets
    {
        /// <summary>
        /// An internal instance
        /// </summary>
        public static Assets Instance
        {
            get
            {
                return _instance;
            }
        }

        public static bool Precompiled
        {
            get
            {
                return _precompiled;
            }
        }

        /// <summary>
        /// A number between 0 and 9. 0 for no compression, 1 for best speed, and 9 for best compression
        /// </summary>
        public static int AssetTextureCompressionLevel
        {
            get
            {
                return _textureCompressionLevel;
            }
            set
            {
                value = Math.Clamp(value, 0, 9);
                _textureCompressionLevel = value;
            }
        }

        private static List<TextureAsset> _textureAssets = new();
        private static List<AudioAsset> _audioAssets = new();
        private static List<ShaderAsset> _shaderAssets = new();
        private static readonly List<DataAsset> _scenes = new();

        private static bool _precompiled;
        private static int _textureCompressionLevel = 9;

        private static Assets _instance = new Assets();

        public static void LoadAssets()
        {
            // Get the texture compression level setting
            if (Settings.GetSetting("TextureCompressionLevel", out SettingOption texCompLevelOption, SettingType.Int)) _textureCompressionLevel = texCompLevelOption.GetValue<int>();

            // Defining Texture.White and Texture.None
            Texture.White = new Texture(1, 1, new byte[] { 255, 255, 255, 255 });
            Texture.None = new Texture(1, 1, new byte[] { 0, 0, 0, 0 });

            // Get the asset compiler check sum setting
            if (Settings.GetSetting("AssetCheckSum", out SettingOption checkSumSetting, SettingType.Bool)) AssetCompiler.compareCheckSum = checkSumSetting.GetValue<bool>();

            //  Whether the application is precompiled
            if (Settings.Precompiled)
            {
                Debug.LogStatus("Running on precompiled assets");
                _precompiled = true;

                float startTime = Time.GameTime;

                // Loads only precompiled assets
                LoadPrecompiledAssets();

                float loadingTime = Time.GameTime - startTime;
                Debug.LogStatus($"Precompiled assets loaded in {Debug.FloatToShortString(loadingTime)} seconds");
            }
            else
            {
                Debug.LogStatus("Running on dynamic assets");
                _precompiled = false;

                float startTime = Time.GameTime;

                // Starts a recursive loop of searching directories in the "Assets" folder
                // Responsible for loading all dynamic assets
                AssetSearchDirectory(Debug.assetsPath);

                float loadingTime = Time.GameTime - startTime;
                Debug.LogStatus($"Dynamic assets loaded in {Debug.FloatToShortString(loadingTime)} seconds");

                // Compile assets if specified in settings
                if (Settings.GetSetting("PrecompileAssets", out SettingOption precompileAssetsOption, SettingType.Bool) && precompileAssetsOption.GetValue<bool>())
                {
                    Debug.WriteToConsole($"Precompiling all assets...", ConsoleColor.Blue);

                    // Compile data
                    AssetCompilationInfo info = CompileDataAssets();

                    // Create a compilation log
                    using (FileStream fileStream = File.Create($"{Debug.logFolderPath}{Time.CurrentDateTimeShortString} CompilationLog.log"))
                    {
                        string final = "Compilation log:\n";
                        final += info.ToString();

                        fileStream.Write(Encoding.Unicode.GetBytes(final));
                    }

                    // Write to a log file and console
                    Debug.WriteToLogFile("\nAsset compilation:\n" + info, LogSeverity.Custom);
                    Debug.WriteToConsole("Compilation:\n" + info, ConsoleColor.Blue);
                }
            }

            // Set the missing texture
            Texture.Missing = GetTexture("Missing");
            Texture.Missing ??= Texture.None;

            Shader.LoadDefaultShaders();

            GC.Collect();
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
        public static void LoadDynamicAsset(string path)
        {
            if (!File.Exists(path)) return;

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

        private static void AssetSearchDirectory(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);

            for (int i = 0; i < files.Length; i++)
            {
                LoadDynamicAsset(files[i]);
            }
            for (int i = 0; i < directories.Length; i++)
            {
                AssetSearchDirectory(directories[i]);
            }
        }
        private static AssetCompilationInfo CompileDataAssets()
        {
            float startTime = Time.GameTime;
            long textureSize, audioSize, shaderSize;

            // Compiling texture data
            string texturePath = Debug.runtimeAssetsPath + "RuntimeTextures.rtmAsset";

            using (FileStream textureFileStream = File.Create(texturePath))
            {
                foreach (TextureAsset asset in _textureAssets)
                {
                    textureFileStream.Write(AssetCompiler.CompileTextureAsset(asset));
                }
                textureSize = textureFileStream.Length;
            }

            // Compiling audio data
            string audioPath = Debug.runtimeAssetsPath + "RuntimeAudioClips.rtmAsset";
            using (FileStream audioFileStream = File.Create(audioPath))
            {
                foreach (AudioAsset asset in _audioAssets)
                {
                    audioFileStream.Write(AssetCompiler.CompileAudioAsset(asset));
                }
                audioSize = audioFileStream.Length;
            }

            // Compiling shaders
            string shaderPath = Debug.runtimeAssetsPath + "RuntimeShaders.rtmAsset";
            using (FileStream shaderFileStream = File.Create(shaderPath))
            {
                foreach (ShaderAsset asset in _shaderAssets)
                {
                    shaderFileStream.Write(AssetCompiler.CompileShaderAsset(asset));
                }
                shaderSize = shaderFileStream.Length;
            }

            // Compiling setttings
            string settingsPath = Debug.runtimeAssetsPath + "RuntimeSettings.rtmAsset";
            using (FileStream settingsFileStream = File.Create(settingsPath))
            {
                settingsFileStream.Write(AssetCompiler.CompileData("GLOBALSETTINGS", Encoding.Unicode.GetBytes(Settings.SettingsText)));
            }

            float compilationTime = Time.GameTime - startTime;

            return new AssetCompilationInfo(textureSize, audioSize, shaderSize, compilationTime);

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

    public class AssetCompilationInfo
    {
        public readonly float compilationTime;
        public readonly long textureCompressedSize;
        public readonly long audioCompressedSize;
        public readonly long shaderSize;

        public AssetCompilationInfo(long textureCompressedSize, long audioCompressedSize, long shaderSize, float compilationTime)
        {
            this.compilationTime = compilationTime;
            this.textureCompressedSize = textureCompressedSize;
            this.audioCompressedSize = audioCompressedSize;
            this.shaderSize = shaderSize;
        }

        public override string ToString()
        {
            string final = $"Textures: {Debug.ByteCountToString(UsageProfiler.TextureMemoryUsage)} raw, {Debug.ByteCountToString(textureCompressedSize)} compressed\n";
            final += $"Audio: {Debug.ByteCountToString(UsageProfiler.AudioMmeoryUsage)} raw, {Debug.ByteCountToString(audioCompressedSize)} compressed\n";
            final += $"Shaders: {Debug.ByteCountToString(shaderSize)}\n";
            final += $"Compilation Time: {Debug.FloatToShortString(compilationTime)} seconds";
            return final;
        }
    }
}
