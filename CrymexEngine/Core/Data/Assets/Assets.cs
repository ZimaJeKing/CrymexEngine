using CrymexEngine.Data;
using CrymexEngine.Debugging;
using CrymexEngine.Rendering;
using CrymexEngine.Utils;
using SixLabors.Fonts;
using System.Text;
using SysPath = System.IO.Path;

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

        public static bool Precompiled => _precompiled;
        public static bool UseMeta => _useMeta;

        /// <summary>
        /// A number between 0 and 9. 0 for no compression. 1 for best speed, and 9 for best compression
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

        public static FontFamily DefaultFontFamily => _defaultFontFamily;

        private static List<TextureAsset> _textureAssets = new();
        private static List<AudioAsset> _audioAssets = new();
        private static List<ShaderAsset> _shaderAssets = new();
        private static List<FontAsset> _fontAssets = new();
        private static readonly List<DataAsset> _scenes = new();

        private static readonly FontCollection _fontCollecion = new();
        private static bool _useMeta = false;
        private static bool _precompiled;
        private static int _textureCompressionLevel = 9;

        private static FontFamily _defaultFontFamily;

        private static readonly Assets _instance = new Assets();

        public static void LoadAssets()
        {
            if (Window.Loaded) return;

            // Get the texture compression level setting
            if (Settings.GetSetting("TextureCompressionLevel", out SettingOption texCompLevelSetting, SettingType.Int))
            {
                _textureCompressionLevel = texCompLevelSetting.GetValue<int>();
            }

            // Get the meta file setting
            if (Settings.GetSetting("UseMetaFiles", out SettingOption useMetaSetting, SettingType.Bool) && useMetaSetting.GetValue<bool>())
            {
                _useMeta = true;
            }

            // Defining Texture.White and Texture.None
            Texture.White = new Texture(1, 1, new byte[] { 255, 255, 255, 255 });
            Texture.None = new Texture(1, 1, new byte[] { 0, 0, 0, 0 });

            //  Whether the application is precompiled
            if (Settings.Instance.Precompiled)
            {
                Debug.LogLocalInfo("Asset Loader", "Running on precompiled assets");
                _precompiled = true;

                float startTime = Time.GameTime;

                // Loads only precompiled assets
                LoadPrecompiledAssets();

                float loadingTime = Time.GameTime - startTime;
                Debug.LogLocalInfo("Asset Loader", $"Precompiled assets loaded in {DataUtilities.FloatToShortString(loadingTime, 2)} seconds");
            }
            else
            {
                Debug.LogLocalInfo("Asset Loader", "Running on dynamic assets");
                _precompiled = false;

                float startTime = Time.GameTime;

                // Starts a recursive loop of searching directories in the "Assets" folder
                // Responsible for loading all dynamic assets
                AssetSearchDirectory(IO.assetsPath);

                float loadingTime = Time.GameTime - startTime;
                Debug.LogLocalInfo("Asset Loader", $"Dynamic assets loaded in {DataUtilities.FloatToShortString(loadingTime, 2)} seconds");

                // Compile assets if specified in settings
                if (Settings.GetSetting("PrecompileAssets", out SettingOption precompileAssetsOption, SettingType.Bool) && precompileAssetsOption.GetValue<bool>())
                {
                    Debug.WriteToConsole($"Precompiling all assets...", ConsoleColor.Blue);

                    // Compile data
                    AssetCompilationInfo info = CompileDataAssets();

                    // Create a compilation log
                    using (FileStream fileStream = File.Create($"{IO.logFolderPath}{Time.CurrentDateTimeShortString} CompilationLog.log"))
                    {
                        string final = "Compilation log:\n";
                        final += info.ToString();

                        fileStream.Write(Encoding.Unicode.GetBytes(final));
                    }

                    // Write to a log file and console
                    Debug.WriteToLogFile("\nAsset compilation:\n" + info, LogSeverity.Custom);
                    Debug.WriteToConsole("Compilation:\n" + info, ConsoleColor.Blue);

                    // End the session
                    Engine.Quit();
                }
            }

            // Set the missing texture
            Texture.Missing = GetTexture("Missing");
            Texture.Missing ??= Texture.None;

            Shader.LoadDefaultShaders();

            GC.Collect();
        }

        /// <summary>
        /// Gets a loaded compressed texture and decompresses it. Try using 'Assets.GetTextureCompressed' if you don't plan on modifying it
        /// </summary>
        public static Texture GetTexture(string name)
        {
            foreach (TextureAsset asset in _textureAssets)
            {
                if (asset.name == name)
                {
                    return asset.texture;
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
        public static FontFamily GetFontFamily(string name)
        {
            foreach (FontAsset asset in _fontAssets)
            {
                if (asset.name == name)
                {
                    return asset.family;
                }
            }
            return _defaultFontFamily;
        }

        /// <summary>
        /// Loads a dynamic asset into the Assets registry
        /// </summary>
        public static void LoadDynamicAsset(string path)
        {
            if (!File.Exists(path)) return;

            string filename = SysPath.GetFileNameWithoutExtension(path);
            string fileExtension = SysPath.GetExtension(path).ToLower();

            switch (fileExtension.ToLower())
            {
                case ".png":
                    {
                        MetaFile meta = MetaFileManager.DecodeMetaFromFile(path + ".meta");
                        TextureAsset texAsset = new TextureAsset(path, Texture.Load(path, meta), meta);
                        _textureAssets.Add(texAsset);
                        break;
                    }
                case ".jpg":
                    {
                        MetaFile meta = MetaFileManager.DecodeMetaFromFile(path + ".meta");
                        TextureAsset texAsset = new TextureAsset(path, Texture.Load(path, meta), meta);
                        _textureAssets.Add(texAsset); break;
                    }
                case ".jpeg":
                    {
                        MetaFile meta = MetaFileManager.DecodeMetaFromFile(path + ".meta");
                        TextureAsset texAsset = new TextureAsset(path, Texture.Load(path, meta), meta);
                        _textureAssets.Add(texAsset); break;
                    }
                case ".bmp":
                    {
                        MetaFile meta = MetaFileManager.DecodeMetaFromFile(path + ".meta");
                        TextureAsset texAsset = new TextureAsset(path, Texture.Load(path, meta), meta); 
                        _textureAssets.Add(texAsset); break;
                    }
                case ".gif":
                    {
                        MetaFile meta = MetaFileManager.DecodeMetaFromFile(path + ".meta");
                        TextureAsset texAsset = new TextureAsset(path, Texture.Load(path, meta), meta);
                        _textureAssets.Add(texAsset); break;
                    }
                case ".wav":
                    {
                        AudioClip? clip = AudioClip.Load(path);
                        if (clip == null) break;
                        AudioAsset audioAsset = new AudioAsset(path, clip, MetaFileManager.DecodeMetaFromFile(path + ".meta"));
                        _audioAssets.Add(audioAsset);
                        break;
                    }
                case ".mp3":
                    {
                        AudioClip? clip = AudioClip.Load(path);
                        if (clip == null) break;
                        AudioAsset audioAsset = new AudioAsset(path, clip, MetaFileManager.DecodeMetaFromFile(path + ".meta"));
                        _audioAssets.Add(audioAsset);
                        break;
                    }
                case ".scene":
                    {
                        _scenes.Add(new DataAsset(path));
                        break;
                    }
                case ".vertex":
                    {
                        string fragmentPath = SysPath.GetDirectoryName(path) + '\\' + filename + ".fragment";
                        if (File.Exists(fragmentPath))
                        {
                            string vertexCode = File.ReadAllText(path);
                            string fragmentCode = File.ReadAllText(fragmentPath);
                            ShaderAsset shaderAsset = new ShaderAsset(path, Shader.LoadFromAsset(vertexCode, fragmentCode), vertexCode, fragmentCode, MetaFileManager.DecodeMetaFromFile(path + ".meta"));
                            _shaderAssets.Add(shaderAsset);
                        }
                        break;
                    }
                case ".ttf":
                    {
                        LoadFontFromData(path, File.ReadAllBytes(path));
                        break;
                    }
                case ".meta":
                    {
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

        public static void LoadFontFromData(string path, byte[] data)
        {
            using MemoryStream fileStream = new MemoryStream(data);

            FontFamily family = _fontCollecion.Add(fileStream);
            _fontAssets.Add(new FontAsset(path, family));
            if (_defaultFontFamily == default) _defaultFontFamily = family;
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
            long textureSize, audioSize, shaderSize, fontSize;

            // Compiling texture data
            string texturePath = IO.runtimeAssetsPath + "RuntimeTextures.rtmAsset";

            using (FileStream textureFileStream = File.Create(texturePath))
            {
                foreach (TextureAsset asset in _textureAssets)
                {
                    textureFileStream.Write(AssetCompiler.CompileTextureAsset(asset));
                }
                textureSize = textureFileStream.Length;
            }

            // Compiling audio data
            string audioPath = IO.runtimeAssetsPath + "RuntimeAudioClips.rtmAsset";
            using (FileStream audioFileStream = File.Create(audioPath))
            {
                foreach (AudioAsset asset in _audioAssets)
                {
                    audioFileStream.Write(AssetCompiler.CompileAudioAsset(asset));
                }
                audioSize = audioFileStream.Length;
            }

            // Compiling shaders
            string shaderPath = IO.runtimeAssetsPath + "RuntimeShaders.rtmAsset";
            using (FileStream shaderFileStream = File.Create(shaderPath))
            {
                foreach (ShaderAsset asset in _shaderAssets)
                {
                    shaderFileStream.Write(AssetCompiler.CompileShaderAsset(asset));
                }
                shaderSize = shaderFileStream.Length;
            }

            // Compiling fonts
            string fontPath = IO.runtimeAssetsPath + "RuntimeFonts.rtmAsset";
            using (FileStream fontFileStream = File.Create(fontPath))
            {
                foreach (FontAsset asset in _fontAssets)
                {
                    fontFileStream.Write(AssetCompiler.CompileData(asset.name, File.ReadAllBytes(asset.path)));
                }
                fontSize = fontFileStream.Length;
            }

            // Compiling setttings
            string settingsPath = IO.runtimeAssetsPath + "RuntimeSettings.rtmAsset";
            using (FileStream settingsFileStream = File.Create(settingsPath))
            {
                settingsFileStream.Write(AssetCompiler.CompileData("GLOBALSETTINGS", Encoding.Unicode.GetBytes(Settings.SettingsText)));
            }

            float compilationTime = Time.GameTime - startTime;

            return new AssetCompilationInfo(textureSize, audioSize, shaderSize, fontSize, compilationTime);
        }
        private static void LoadPrecompiledAssets()
        {
            string texturePath = IO.runtimeAssetsPath + "RuntimeTextures.rtmAsset";
            if (File.Exists(texturePath)) _textureAssets = AssetCompiler.DecompileTextureAssets(File.ReadAllBytes(texturePath));

            string audioPath = IO.runtimeAssetsPath + "RuntimeAudioClips.rtmAsset";
            if (File.Exists(audioPath)) _audioAssets = AssetCompiler.DecompileAudioAssets(File.ReadAllBytes(audioPath));

            string fontPath = IO.runtimeAssetsPath + "RuntimeFonts.rtmAsset";
            if (File.Exists(fontPath)) _fontAssets = AssetCompiler.DecompileFontAssets(File.ReadAllBytes(fontPath));

            string shaderPath = IO.runtimeAssetsPath + "RuntimeShaders.rtmAsset";
            if (File.Exists(shaderPath)) _shaderAssets = AssetCompiler.DecompileShaderAssets(File.ReadAllBytes(shaderPath));
        }
    }
}
