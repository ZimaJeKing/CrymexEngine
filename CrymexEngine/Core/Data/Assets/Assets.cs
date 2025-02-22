using CrymexEngine.Data;
using CrymexEngine.Debugging;
using CrymexEngine.Rendering;
using CrymexEngine.Utils;
using SixLabors.Fonts;
using System.Text;
using SysPath = System.IO.Path;

namespace CrymexEngine
{
    public static class Assets
    {
        public static bool RunningPrecompiled => _runningPrecompiled;
        public static bool UseMeta => _useMeta;
        public static bool Loaded => _loaded;

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
        private static bool _loaded = false;
        private static bool _runningPrecompiled;
        private static int _textureCompressionLevel = 9;

        private static FontFamily _defaultFontFamily;

        internal static void LoadAssets()
        {
            _loaded = true;

            LoadSettings();

            // Defining Texture.White and Texture.None
            DefineTextures();

            //  Whether the application is precompiled
            if (Settings.GlobalSettings.Precompiled)
            {
                FullyLoadPrecompiled();
            }
            else
            {
                FullyLoadDynamic();

                // Compile assets if specified in settings
                if (Settings.GlobalSettings.GetSetting("PrecompileAssets", out SettingOption precompileAssetsOption, SettingType.Bool) && precompileAssetsOption.GetValue<bool>())
                {
                    CompileAssets();
                }
            }

            LoadMissingTexture();

            Shader.LoadDefaultShaders();

            GC.Collect();
        }

        private static void LoadMissingTexture()
        {
            // Set the missing texture
            string missingTextureName = "Missing";

            if (Settings.GlobalSettings.GetSetting("MissingTexture", out SettingOption missingTextureOption, SettingType.RefString))
            {
                missingTextureName = missingTextureOption.GetValue<string>();
            }

            Texture.Missing = GetTextureBroad(missingTextureName);
            if (Texture.Missing == null)
            {
                // Define a basic missing texture with raw data
                Texture.Missing = new Texture(2, 2, new byte[] { 255, 0, 255, 255, 0, 0, 0, 255, 0, 0, 0, 255, 255, 0, 255, 255 });
            }
        }

        private static void FullyLoadDynamic()
        {
            Debug.LogLocalInfo("Asset Loader", "Running on dynamic assets");
            _runningPrecompiled = false;

            float startTime = Time.GameTime;

            // Starts a recursive loop of searching directories in the "Assets" folder
            // Responsible for loading all dynamic assets
            AssetSearchDirectory(Directories.AssetsPath);

            float loadingTime = Time.GameTime - startTime;
            Debug.LogLocalInfo("Asset Loader", $"Dynamic assets loaded in {DataUtilities.FloatToShortString(loadingTime, 2)} seconds");
        }

        private static void CompileAssets()
        {
            Debug.WriteToConsole($"Precompiling all assets...", ConsoleColor.Blue);

            // Compile data
            AssetCompilationInfo info = CompileDataAssets();

            // Create a compilation log
            using (FileStream fileStream = File.Create($"{Directories.LogFolderPath}{Time.CurrentDateTimeShortString} CompilationLog.log"))
            {
                string final = "Compilation log:\n";
                final += info.ToString();

                fileStream.Write(Encoding.Unicode.GetBytes(final));
            }

            // Write to a log file and console
            Debug.Log("Asset compilation successful", ConsoleColor.Blue);
            Debug.WriteToLogFile("\nAsset compilation:\n" + info, LogSeverity.Custom);
            Debug.WriteToConsole("Compilation:\n" + info, ConsoleColor.Blue);

            // End the session
            Engine.Quit();
        }

        private static void FullyLoadPrecompiled()
        {
            Debug.LogLocalInfo("Asset Loader", "Running on precompiled assets");
            _runningPrecompiled = true;

            float startTime = Time.GameTime;

            // Loads only precompiled assets
            LoadPrecompiledAssets();

            float loadingTime = Time.GameTime - startTime;
            Debug.LogLocalInfo("Asset Loader", $"Precompiled assets loaded in {DataUtilities.FloatToShortString(loadingTime, 2)} seconds");
        }

        private static void DefineTextures()
        {
            Texture.White = new Texture(1, 1, new byte[] { 255, 255, 255, 255 });
            Texture.None = new Texture(1, 1, new byte[] { 0, 0, 0, 0 });
        }

        private static void LoadSettings()
        {
            // Get the texture compression level setting
            if (Settings.GlobalSettings.GetSetting("TextureCompressionLevel", out SettingOption texCompLevelSetting, SettingType.Int))
            {
                _textureCompressionLevel = texCompLevelSetting.GetValue<int>();
            }

            // Get the meta file setting
            if (Settings.GlobalSettings.GetSetting("UseMetaFiles", out SettingOption useMetaSetting, SettingType.Bool) && useMetaSetting.GetValue<bool>())
            {
                _useMeta = true;
            }
        }

        public static TextureAsset? GetTextureAsset(string name)
        {
            foreach (TextureAsset asset in _textureAssets)
            {
                if (asset.name == name)
                {
                    return asset;
                }
            }
            return null;
        }
        public static ShaderAsset? GetShaderAsset(string name)
        {
            foreach (ShaderAsset asset in _shaderAssets)
            {
                if (asset.name == name)
                {
                    return asset;
                }
            }
            return null;
        }
        public static AudioAsset? GetAudioAsset(string name)
        {
            foreach (AudioAsset asset in _audioAssets)
            {
                if (asset.name == name)
                {
                    return asset;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a texture containing the argument string
        /// </summary>
        public static Texture GetTextureBroad(string name)
        {
            foreach (TextureAsset asset in _textureAssets)
            {
                if (asset.name.Contains(name))
                {
                    return asset.texture;
                }
            }
            return Texture.Missing;
        }
        /// <summary>
        /// Gets a texture with the same name as the argument
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
        /// <summary>
        /// Gets an audio clip with the same name as the argument
        /// </summary>
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
        /// <summary>
        /// Gets a shader containing the argument string
        /// </summary>
        public static Shader GetShaderBroad(string name)
        {
            foreach (ShaderAsset asset in _shaderAssets)
            {
                if (asset.name.Contains(name))
                {
                    return asset.shader;
                }
            }
            return null;
        }
        /// <summary>
        /// Gets a shader with the same name as the argument
        /// </summary>
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
                        else
                        {
                            Debug.LogWarning($"Fragment shader not found for '{filename + fileExtension}'");
                        }
                        break;
                    }
                case ".vert":
                    {
                        string fragmentPath = SysPath.GetDirectoryName(path) + '\\' + filename + ".frag";
                        if (File.Exists(fragmentPath))
                        {
                            string vertexCode = File.ReadAllText(path);
                            string fragmentCode = File.ReadAllText(fragmentPath);
                            ShaderAsset shaderAsset = new ShaderAsset(path, Shader.LoadFromAsset(vertexCode, fragmentCode), vertexCode, fragmentCode, MetaFileManager.DecodeMetaFromFile(path + ".meta"));
                            _shaderAssets.Add(shaderAsset);
                        }
                        else
                        {
                            Debug.LogWarning($"Fragment shader not found for '{filename + fileExtension}'");
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
            string texturePath = Directories.RuntimeAssetsPath + "RuntimeTextures.rtmAsset";

            using (FileStream textureFileStream = File.Create(texturePath))
            {
                foreach (TextureAsset asset in _textureAssets)
                {
                    bool? includeInRelease = asset.Meta.GetBoolProperty("IncludeInRelease");
                    if (asset.Meta == null || includeInRelease != null && includeInRelease.Value)
                    {
                        textureFileStream.Write(AssetCompiler.CompileTextureAsset(asset));
                    }
                }
                textureSize = textureFileStream.Length;
            }

            // Compiling audio data
            string audioPath = Directories.RuntimeAssetsPath + "RuntimeAudioClips.rtmAsset";
            using (FileStream audioFileStream = File.Create(audioPath))
            {
                foreach (AudioAsset asset in _audioAssets)
                {
                    bool? includeInRelease = asset.Meta.GetBoolProperty("IncludeInRelease");
                    if (asset.Meta == null || includeInRelease != null && includeInRelease.Value)
                    {
                        audioFileStream.Write(AssetCompiler.CompileAudioAsset(asset));
                    }
                }
                audioSize = audioFileStream.Length;
            }

            // Compiling shaders
            string shaderPath = Directories.RuntimeAssetsPath + "RuntimeShaders.rtmAsset";
            using (FileStream shaderFileStream = File.Create(shaderPath))
            {
                foreach (ShaderAsset asset in _shaderAssets)
                {
                    bool? includeInRelease = asset.Meta.GetBoolProperty("IncludeInRelease");
                    if (asset.Meta == null || includeInRelease != null && includeInRelease.Value)
                    {
                        shaderFileStream.Write(AssetCompiler.CompileShaderAsset(asset));
                    }
                }
                shaderSize = shaderFileStream.Length;
            }

            // Compiling fonts
            string fontPath = Directories.RuntimeAssetsPath + "RuntimeFonts.rtmAsset";
            using (FileStream fontFileStream = File.Create(fontPath))
            {
                foreach (FontAsset asset in _fontAssets)
                {
                    bool? includeInRelease = asset.Meta?.GetBoolProperty("IncludeInRelease");
                    if (asset.Meta == null || includeInRelease != null && includeInRelease.Value)
                    {
                        fontFileStream.Write(AssetCompiler.CompileData(asset.name, File.ReadAllBytes(asset.path)));
                    }
                }
                fontSize = fontFileStream.Length;
            }

            // Compiling setttings
            string settingsPath = Directories.RuntimeAssetsPath + "GlobalSettings.settingsFile";
            using (FileStream settingsFileStream = File.Create(settingsPath))
            {
                settingsFileStream.Write(AssetCompiler.CompileData("GLOBALSETTINGS", Encoding.Unicode.GetBytes(Settings.GlobalSettings.SettingsText)));
            }

            float compilationTime = Time.GameTime - startTime;

            return new AssetCompilationInfo(textureSize, audioSize, shaderSize, fontSize, compilationTime);
        }
        private static void LoadPrecompiledAssets()
        {
            string texturePath = Directories.RuntimeAssetsPath + "RuntimeTextures.rtmAsset";
            if (File.Exists(texturePath)) _textureAssets = AssetCompiler.DecompileTextureAssets(File.ReadAllBytes(texturePath));

            string audioPath = Directories.RuntimeAssetsPath + "RuntimeAudioClips.rtmAsset";
            if (File.Exists(audioPath)) _audioAssets = AssetCompiler.DecompileAudioAssets(File.ReadAllBytes(audioPath));

            string fontPath = Directories.RuntimeAssetsPath + "RuntimeFonts.rtmAsset";
            if (File.Exists(fontPath)) _fontAssets = AssetCompiler.DecompileFontAssets(File.ReadAllBytes(fontPath));

            string shaderPath = Directories.RuntimeAssetsPath + "RuntimeShaders.rtmAsset";
            if (File.Exists(shaderPath)) _shaderAssets = AssetCompiler.DecompileShaderAssets(File.ReadAllBytes(shaderPath));
        }
    }
}
