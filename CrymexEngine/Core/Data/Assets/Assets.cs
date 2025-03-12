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
        public static int TextureCompressionLevel
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

        private static Dictionary<string, TextureAsset> _textures = new();
        private static Dictionary<string, AudioAsset> _audioClips = new();
        private static Dictionary<string, ShaderAsset> _shaders = new();
        private static Dictionary<string, FontAsset> _fonts = new();
        private static Dictionary<string, SettingAsset> _settings = new();

        private static readonly FontCollection _fontCollecion = new();
        private static bool _useMeta = false;
        private static bool _loaded = false;
        private static bool _runningPrecompiled;
        private static int _textureCompressionLevel = 9;

        private static FontFamily _defaultFontFamily;
        private static bool _recompileSettings;

        internal static void LoadAssets()
        {
            _loaded = true;

            LoadSettings();

            // Defining Texture.Missing, Texture.White and Texture.None
            DefineTextures();

            //  Whether the application is precompiled
            if (Engine.Precompiled)
            {
                StartLoadingPrecompiled();
            }
            else
            {
                StartLoadingDynamic();

                // Compile assets if specified in settings
                if (Settings.GlobalSettings.GetSetting("PrecompileAssets", out SettingOption precompileAssetsOption, SettingType.Bool) && precompileAssetsOption.GetValue<bool>())
                {
                    CompileAssets();
                }
            }

            Shader.LoadDefaultShaders();

            GC.Collect();
        }

        public static TextureAsset? GetTextureAsset(string name)
        {
            if (_textures.TryGetValue(name, out TextureAsset? asset))
            {
                return asset;
            }
            return null;
        }

        public static ShaderAsset? GetShaderAsset(string name)
        {
            if (_shaders.TryGetValue(name, out ShaderAsset? asset))
            {
                return asset;
            }
            return null;
        }

        public static AudioAsset? GetAudioAsset(string name)
        {
            if (_audioClips.TryGetValue(name, out AudioAsset? asset))
            {
                return asset;
            }
            return null;
        }

        /// <summary>
        /// Gets a texture with the same name as the argument
        /// </summary>
        public static Texture GetTexture(string name)
        {
            if (_textures.TryGetValue(name, out TextureAsset? asset))
            {
                if (asset != null) return asset.texture;
            }
            return Texture.Missing;
        }

        /// <summary>
        /// Gets an audio clip containing the name
        /// </summary>
        public static AudioClip GetAudioClip(string name)
        {
            if (_audioClips.TryGetValue(name, out AudioAsset? asset))
            {
                if (asset != null) return asset.clip;
            }
            return null;
        }

        /// <summary>
        /// Gets a shader with the same name as the argument
        /// </summary>
        public static Shader? GetShader(string name)
        {
            if (_shaders.TryGetValue(name, out ShaderAsset? asset))
            {
                if (asset != null) return asset.shader;
            }
            return null;
        }

        public static FontFamily GetFontFamily(string name)
        {
            if (_fonts.TryGetValue(name, out FontAsset? asset))
            {
                if (asset != null) return asset.family;
            }
            return _defaultFontFamily;
        }

        public static Settings? GetSettings(string name)
        {
            if (_settings.TryGetValue(name, out SettingAsset? asset))
            {
                if (asset != null) return asset.settings;
            }
            return null;
        }

        /// <summary>
        /// Gets an audio clip with the same name in any subdirectory
        /// </summary>
        public static AudioClip? GetAudioClipBroad(string name)
        {
            var similarKey = _audioClips.Keys.FirstOrDefault(k => BroadComparison(k, name));

            if (similarKey != null)
            {
                return _audioClips[similarKey].clip;
            }

            return null;
        }
        /// <summary>
        /// Gets a shader with the same name in any subdirectory
        /// </summary>
        public static Shader GetShaderBroad(string name)
        {
            var similarKey = _shaders.Keys.FirstOrDefault(k => BroadComparison(k, name));

            if (similarKey != null)
            {
                return _shaders[similarKey].shader;
            }

            return null;
        }
        /// <summary>
        /// Gets a texture with the same name in any subdirectory
        /// </summary>
        public static Texture GetTextureBroad(string name)
        {
            var similarKey = _textures.Keys.FirstOrDefault(k => BroadComparison(k, name));
            
            if (!string.IsNullOrEmpty(similarKey))
            {
                return _textures[similarKey].texture;
            }

            return Texture.Missing;
        }

        public static Settings GetSettingsBroad(string name)
        {
            var similarKey = _textures.Keys.FirstOrDefault(k => BroadComparison(k, name));

            if (!string.IsNullOrEmpty(similarKey))
            {
                return _settings[similarKey].settings;
            }

            return null;
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
                        LoadTextureAsset(path);
                        break;
                    }
                case ".jpg":
                    {
                        LoadTextureAsset(path);
                        break;
                    }
                case ".jpeg":
                    {
                        LoadTextureAsset(path);
                        break;
                    }
                case ".bmp":
                    {
                        LoadTextureAsset(path);
                        break;
                    }
                case ".gif":
                    {
                        LoadTextureAsset(path);
                        break;
                    }
                case ".wav":
                    {
                        AudioClip? clip = AudioClip.Load(path);
                        if (clip == null) break;
                        AudioAsset audioAsset = new AudioAsset(path, clip, MetaFileManager.DecodeMetaFromFile(path + ".meta"));
                        _audioClips.Add(audioAsset.name, audioAsset);
                        break;
                    }
                case ".mp3":
                    {
                        AudioClip? clip = AudioClip.Load(path);
                        if (clip == null) break;
                        AudioAsset audioAsset = new AudioAsset(path, clip, MetaFileManager.DecodeMetaFromFile(path + ".meta"));
                        _audioClips.Add(audioAsset.name, audioAsset);
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
                            _shaders.Add(shaderAsset.name, shaderAsset);
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
                            _shaders.Add(shaderAsset.name, shaderAsset);
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
                case ".cfg":
                    {
                        if (BroadComparisonPath(path, "CEConfig"))
                        {
                            break;
                        }

                        Settings settings = new Settings();
                        settings.LoadFile(path);
                        SettingAsset asset = new SettingAsset(path, settings);

                        _settings.Add(asset.name, asset);
                        break;
                    }
            }
        }

        private static void LoadTextureAsset(string path)
        {
            MetaFile meta = MetaFileManager.DecodeMetaFromFile(path + ".meta");
            TextureAsset texAsset = new TextureAsset(path, Texture.Load(path, meta), meta);
            _textures.Add(texAsset.name, texAsset);
        }

        internal static void Cleanup()
        {
            foreach (KeyValuePair<string, AudioAsset> pair in _audioClips.ToArray())
            {
                pair.Value.clip.Dispose();
            }
            foreach (KeyValuePair<string, TextureAsset> pair in _textures.ToArray())
            {
                pair.Value.texture.Dispose();
            }
            foreach (KeyValuePair<string, ShaderAsset> pair in _shaders.ToArray())
            {
                pair.Value.shader.Dispose();
            }

            _audioClips.Clear();
            _textures.Clear();
            _shaders.Clear();
        }

        public static void LoadFontFromData(string path, byte[] data)
        {
            using MemoryStream memoryStream = new MemoryStream(data);

            FontFamily family = _fontCollecion.Add(memoryStream);
            FontAsset asset = new FontAsset(path, family);
            _fonts.Add(asset.name, asset);
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
                foreach (KeyValuePair<string, TextureAsset> pair in _textures.ToArray())
                {
                    MetaFile? meta = pair.Value.Meta;
                    bool? includeInRelease = pair.Value.Meta.GetBoolProperty("IncludeInRelease");
                    if (pair.Value.Meta == null || includeInRelease != null && includeInRelease.Value)
                    {
                        textureFileStream.Write(AssetCompiler.CompileTextureAsset(pair.Value));
                    }
                }
                textureSize = textureFileStream.Length;
            }

            // Compiling audio data
            string audioPath = Directories.RuntimeAssetsPath + "RuntimeAudioClips.rtmAsset";
            using (FileStream audioFileStream = File.Create(audioPath))
            {
                foreach (KeyValuePair<string, AudioAsset> pair in _audioClips.ToArray())
                {
                    MetaFile? meta = pair.Value.Meta;
                    bool? includeInRelease = meta.GetBoolProperty("IncludeInRelease");
                    if (meta == null || includeInRelease != null && includeInRelease.Value)
                    {
                        audioFileStream.Write(AssetCompiler.CompileAudioAsset(pair.Value));
                    }
                }
                audioSize = audioFileStream.Length;
            }

            // Compiling shaders
            string shaderPath = Directories.RuntimeAssetsPath + "RuntimeShaders.rtmAsset";
            using (FileStream shaderFileStream = File.Create(shaderPath))
            {
                foreach (KeyValuePair<string, ShaderAsset> pair in _shaders.ToArray())
                {
                    MetaFile? meta = pair.Value.Meta;
                    bool? includeInRelease = meta.GetBoolProperty("IncludeInRelease");
                    if (meta == null || includeInRelease != null && includeInRelease.Value)
                    {
                        shaderFileStream.Write(AssetCompiler.CompileShaderAsset(pair.Value));
                    }
                }
                shaderSize = shaderFileStream.Length;
            }

            // Compiling fonts
            string fontPath = Directories.RuntimeAssetsPath + "RuntimeFonts.rtmAsset";
            using (FileStream fontFileStream = File.Create(fontPath))
            {
                foreach (KeyValuePair<string, FontAsset> pair in _fonts.ToArray())
                {
                    MetaFile? meta = pair.Value.Meta;
                    bool? includeInRelease = meta?.GetBoolProperty("IncludeInRelease");
                    if (meta == null || includeInRelease != null && includeInRelease.Value)
                    {
                        fontFileStream.Write(AssetCompiler.CompileData(pair.Value.name, File.ReadAllBytes(pair.Value.path)));
                    }
                }
                fontSize = fontFileStream.Length;
            }

            // Compiling setttings
            string settingsPath = Directories.RuntimeAssetsPath + "RuntimeSettings.rtmAsset";
            using (FileStream settingsFileStream = File.Create(settingsPath))
            {
                foreach (KeyValuePair<string, SettingAsset> pair in _settings.ToArray())
                {
                    pair.Value.settings.GenerateSettingsText();
                    settingsFileStream.Write(AssetCompiler.CompileData(pair.Value.name, Encoding.Unicode.GetBytes(pair.Value.settings.AsText)));
                }
            }
            Settings.GlobalSettings.GenerateSettingsText();
            File.WriteAllBytes(Directories.RuntimeAssetsPath + "CEConfig.rtmAsset", AssetCompiler.CompileData("CEConfig", Encoding.Unicode.GetBytes(Settings.GlobalSettings.AsText)));

            float compilationTime = Time.GameTime - startTime;

            return new AssetCompilationInfo(textureSize, audioSize, shaderSize, fontSize, compilationTime);
        }

        private static bool LoadPrecompiledAssets()
        {
            try
            {
                string texturePath = Directories.RuntimeAssetsPath + "RuntimeTextures.rtmAsset";
                if (File.Exists(texturePath)) _textures = AssetCompiler.DecompileTextureAssets(File.ReadAllBytes(texturePath));

                string audioPath = Directories.RuntimeAssetsPath + "RuntimeAudioClips.rtmAsset";
                if (File.Exists(audioPath)) _audioClips = AssetCompiler.DecompileAudioAssets(File.ReadAllBytes(audioPath));

                string fontPath = Directories.RuntimeAssetsPath + "RuntimeFonts.rtmAsset";
                if (File.Exists(fontPath)) _fonts = AssetCompiler.DecompileFontAssets(File.ReadAllBytes(fontPath));

                string shaderPath = Directories.RuntimeAssetsPath + "RuntimeShaders.rtmAsset";
                if (File.Exists(shaderPath)) _shaders = AssetCompiler.DecompileShaderAssets(File.ReadAllBytes(shaderPath));

                string settingsPath = Directories.RuntimeAssetsPath + "RuntimeSettings.rtmAsset";
                if (File.Exists(settingsPath)) _settings = AssetCompiler.DecompileSettingAssets(File.ReadAllBytes(settingsPath));
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occured while loading precompiled assets ({ex.Message}). Running on dynamic assets");
                Cleanup();
                StartLoadingDynamic();
                return false;
            }
            return true;
        }

        private static void StartLoadingDynamic()
        {
            Debug.LogLocalInfo("Asset Loader", "Running on dynamic assets");
            _runningPrecompiled = false;

            float startTime = Time.GameTime;

            // Starts a recursive loop of searching directories in the "Assets" folder
            // Responsible for loading all dynamic assets
            AssetSearchDirectory(Directories.AssetsPath);

            float loadingTime = Time.GameTime - startTime;
            Debug.LogLocalInfo("Asset Loader", $"Dynamic assets loaded in {DataUtil.SecondsToTimeString(loadingTime)}");
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

        private static void StartLoadingPrecompiled()
        {
            Debug.LogLocalInfo("Asset Loader", "Running on precompiled assets");
            _runningPrecompiled = true;

            float startTime = Time.GameTime;

            // Loads only precompiled assets
            if (!LoadPrecompiledAssets())
            {
                return;
            }

            float loadingTime = Time.GameTime - startTime;
            Debug.LogLocalInfo("Asset Loader", $"Precompiled assets loaded in {DataUtil.SecondsToTimeString(loadingTime)}");
        }

        private static void DefineTextures()
        {
            Texture.White = new Texture(1, 1, new byte[] { 255, 255, 255, 255 });
            Texture.None = new Texture(1, 1, new byte[] { 0, 0, 0, 0 });

            // Define the missing texture
            string missingTextureName = "Missing";

            if (Settings.GlobalSettings.GetSetting("MissingTexture", out SettingOption missingTextureOption, SettingType.RefString))
            {
                missingTextureName = missingTextureOption.GetValue<string>();
            }

            Texture.Missing = GetTextureBroad(missingTextureName);

            // If texture not found define a basic missing texture with raw data
            Texture.Missing ??= new Texture(2, 2, new byte[] { 255, 0, 255, 255, 0, 0, 0, 255, 0, 0, 0, 255, 255, 0, 255, 255 });
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

        private static bool BroadComparison(string assetName, string name)
        {
            if (assetName.Contains('/')) return assetName.Substring(assetName.LastIndexOf('/') + 1) == name;
            return assetName == name;
        }

        private static bool BroadComparisonPath(string path, string name)
        {
            name = name + Path.GetExtension(path);
            if (path.Contains('\\')) return path.Substring(path.LastIndexOf('\\') + 1) == name;
            else if (path.Contains('/')) return path.Substring(path.LastIndexOf('/') + 1) == name;
            return path == name;
        }
    }
}
