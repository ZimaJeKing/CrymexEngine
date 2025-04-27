using CrymexEngine.Audio;
using CrymexEngine.Data;
using CrymexEngine.Debugging;
using CrymexEngine.Rendering;
using CrymexEngine.Utils;
using SixLabors.Fonts;
using System.Text;

namespace CrymexEngine
{
    public static class Assets
    {
        public static bool RunningPrecompiled => _precompiled;
        public static bool UseMeta => _useMeta;
        public static bool Loaded => _loaded;

        public static FontFamily DefaultFontFamily => _defaultFontFamily;

        private static Dictionary<string, TextureAsset> _textures = new();
        private static Dictionary<string, AudioAsset> _audioClips = new();
        private static Dictionary<string, ShaderAsset> _shaders = new();
        private static Dictionary<string, FontAsset> _fonts = new();
        private static Dictionary<string, SettingAsset> _settings = new();
        private static Dictionary<string, TextAsset> _texts = new();
        private static Dictionary<string, TextAsset> _scenes = new();

        private static readonly FontCollection _fontCollecion = new();
        private static bool _useMeta = false;
        private static bool _loaded = false;
        private static bool _precompiled;
        private static int _textureCompressionLevel = 1;

        private static FontFamily _defaultFontFamily;

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

        /// <summary>
        /// Gets a texture asset with the same name
        /// </summary>
        public static TextureAsset? GetTextureAsset(string name)
        {
            _textures.TryGetValue(name, out TextureAsset? asset);
            return asset;
        }
        /// <summary>
        /// Gets a shader asset with the same name
        /// </summary>
        public static ShaderAsset? GetShaderAsset(string name)
        {
            _shaders.TryGetValue(name, out ShaderAsset? asset);
            return asset;
        }
        /// <summary>
        /// Gets an audio asset with the same name
        /// </summary>
        public static AudioAsset? GetAudioAsset(string name)
        {
            _audioClips.TryGetValue(name, out AudioAsset? asset);
            return asset;
        }
        /// <summary>
        /// Gets a text asset with the same name
        /// </summary>
        public static TextAsset? GetTextAsset(string name)
        {
            _texts.TryGetValue(name, out TextAsset? asset);
            return asset;
        }
        public static TextAsset? GetSceneAsset(string name)
        {
            _scenes.TryGetValue(name, out TextAsset? asset);
            return asset;
        }
        /// <summary>
        /// Gets a texture with the same name. Returns Texture.Missing if not found
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
        /// Gets an audio clip with the same name
        /// </summary>
        public static AudioClip? GetAudioClip(string name)
        {
            _audioClips.TryGetValue(name, out AudioAsset? asset);
            return asset?.clip;
        }
        /// <summary>
        /// Gets a shader with the same name
        /// </summary>
        public static Shader? GetShader(string name)
        {
            _shaders.TryGetValue(name, out ShaderAsset? asset);
            return asset?.shader;
        }
        /// <summary>
        /// Gets a text with the same name
        /// </summary>
        public static string? GetText(string name)
        {
            _texts.TryGetValue(name, out TextAsset? asset);
            return asset?.text;
        }
        /// <summary>
        /// Gets a settings object with the same name
        /// </summary>
        public static Settings? GetSettings(string name)
        {
            _settings.TryGetValue(name, out SettingAsset? asset);
            return asset?.settings;
        }
        public static FontFamily? GetFontFamily(string name)
        {
            _fonts.TryGetValue(name, out FontAsset? asset);
            return asset?.family;
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
        public static Shader? GetShaderBroad(string name)
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
        /// <summary>
        /// Gets a settings object with the same name in any subdirectory
        /// </summary>
        public static Settings? GetSettingsBroad(string name)
        {
            var similarKey = _settings.Keys.FirstOrDefault(k => BroadComparison(k, name));

            if (!string.IsNullOrEmpty(similarKey))
            {
                return _settings[similarKey].settings;
            }

            return null;
        }
        /// <summary>
        /// Gets a text with the same name in any subdirectory
        /// </summary>
        public static string? GetTextBroad(string name)
        {
            var similarKey = _texts.Keys.FirstOrDefault(k => BroadComparison(k, name));

            if (!string.IsNullOrEmpty(similarKey))
            {
                return _texts[similarKey].text;
            }

            return null;
        }


        /// <summary>
        /// Adds a settings object to the asset registry.
        /// This object will be recompiled with other others on Settings.RecompileAllSettings() if not marked as non-recompilable
        /// </summary>
        public static void RegisterSettings(string name, Settings settings)
        {
            if (_settings.ContainsKey(name))
            {
                Debug.LogWarning($"Asset registry already contains key '{name}'");
                return;
            }

            _settings.Add(name, new SettingAsset(name, settings));
        }
        /// <summary>
        /// Removes a settings object from the asset registry. This object won't be recompiled with the other settings
        /// </summary>
        /// <returns>Whether the operation was successful</returns>
        public static bool UnregisterSettings(string name)
        {
            return _settings.Remove(name);
        }

        internal static void LoadAssets()
        {
            _loaded = true;

            LoadSettings();

            // Whether the application is precompiled
            if (Engine.SettingsPrecompiled)
            {
                StartLoadingPrecompiled();

                // Mixed assets mode
                if (Settings.GlobalSettings.GetSetting("MixedAssetsMode", out SettingOption mixedAssetsOption, SettingType.Bool) && mixedAssetsOption.GetValue<bool>())
                {
                    Debug.LogLocalInfo("Asset Loader", "Running on mixed assets");

                    StartLoadingDynamic();
                }
                else
                {
                    Debug.LogLocalInfo("Asset Loader", "Running on precompiled assets");
                }
            }
            else
            {
                _precompiled = false;
                StartLoadingDynamic();

                // Compile assets if specified in settings
                if (Settings.GlobalSettings.GetSetting("PrecompileAssets", out SettingOption precompileAssetsOption, SettingType.Bool) && precompileAssetsOption.GetValue<bool>())
                {
                    Compile();
                }
            }

            // Defining Texture.Missing, Texture.White and Texture.None
            Texture.InitBaseTextures();

            Shader.LoadDefaultShaders();

            GC.Collect();
        }

        internal static void LoadFontFromData(string path, byte[] data)
        {
            using MemoryStream memoryStream = new MemoryStream(data);

            FontFamily family = _fontCollecion.Add(memoryStream);
            FontAsset asset = new FontAsset(path, family);
            _fonts.Add(asset.name, asset);
            if (_defaultFontFamily == default) _defaultFontFamily = family;
        }

        internal static void Cleanup()
        {
            foreach (KeyValuePair<string, AudioAsset> pair in _audioClips.ToArray())
            {
                pair.Value?.clip?.Dispose();
            }
            foreach (KeyValuePair<string, TextureAsset> pair in _textures.ToArray())
            {
                pair.Value?.texture?.Dispose();
            }
            foreach (KeyValuePair<string, ShaderAsset> pair in _shaders.ToArray())
            {
                pair.Value?.shader?.Dispose();
            }

            _audioClips.Clear();
            _textures.Clear();
            _shaders.Clear();
        }

        internal static KeyValuePair<string, SettingAsset>[] GetAllSettingAssets() => _settings.ToArray();

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

        /// <summary>
        /// Loads a dynamic asset file into the Assets registry
        /// </summary>
        private static void LoadDynamicAsset(string path)
        {
            if (!File.Exists(path)) return;

            string fileExtension = Path.GetExtension(path).ToLower();

            switch (fileExtension.ToLower())
            {
                case ".png": // Textures
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
                case ".webp":
                    {
                        LoadTextureAsset(path);
                        break;
                    }
                case ".wav": // Audio
                    {
                        LoadAudioAsset(path);
                        break;
                    }
                case ".ogg":
                    {
                        LoadAudioAsset(path);
                        break;
                    }
                case ".vertex": // Shaders
                    {
                        LoadShaderAsset(path);
                        break;
                    }
                case ".vert":
                    {
                        LoadShaderAsset(path);
                        break;
                    }
                case ".ttf": // Fonts
                    {
                        LoadFontAsset(path);
                        break;
                    }
                case ".otf":
                    {
                        LoadFontAsset(path);
                        break;
                    }
                case ".ttc":
                    {
                        LoadFontAsset(path);
                        break;
                    }
                case ".woff":
                    {
                        LoadFontAsset(path);
                        break;
                    }
                case ".woff2":
                    {
                        LoadFontAsset(path);
                        break;
                    }
                case ".txt": // Texts
                    {
                        LoadTextAsset(path);
                        break;
                    }
                case ".json":
                    {
                        LoadTextAsset(path);
                        break;
                    }
                case ".meta": // Meta
                    {
                        // Delete meta file if original doesn't exist
                        if (!File.Exists(path[0..^5]))
                        {
                            Debug.LogWarning($"The original file for '{Path.GetFileName(path)}' not found. Meta file deleted");
                            File.Delete(path);
                        }
                        break;
                    }
                case ".cfg": // Settings
                    {
                        LoadSettingsAsset(path); 
                        break;
                    }
                case ".scene": // Scenes
                    {
                        LoadSceneAsset(path);
                        break;
                    }
            }
        }

        private static void LoadShaderAsset(string path)
        {
            string assetName = DataUtil.GetCENameFromPath(path);

            if (_shaders.ContainsKey(assetName)) return;

            string vertexFileExtension = Path.GetExtension(path);
            string fragmentFileExtension;
            switch (vertexFileExtension)
            {
                case ".vert": 
                    fragmentFileExtension = ".frag"; 
                    break;
                default:
                    fragmentFileExtension = ".fragment";
                    break;
            }

            string? fragmentPath = Path.ChangeExtension(path, fragmentFileExtension);
            if (File.Exists(fragmentPath))
            {
                string vertexCode = File.ReadAllText(path);
                string fragmentCode = File.ReadAllText(fragmentPath);
                ShaderAsset shaderAsset = new ShaderAsset(path, Shader.LoadFromAsset(vertexCode, fragmentCode), vertexCode, fragmentCode);
                _shaders.Add(shaderAsset.name, shaderAsset);
            }
            else
            {
                Debug.LogWarning($"Fragment shader not found for shader '{fragmentPath}'");
            }
        }
        private static void LoadTextureAsset(string path)
        {
            string assetName = DataUtil.GetCENameFromPath(path);

            if (_textures.ContainsKey(assetName)) return;

            MetaFile meta = MetaFileManager.DecodeMetaFromFile(path + ".meta");
            TextureAsset texAsset = new TextureAsset(path, Texture.Load(path, meta), meta);
            _textures.Add(texAsset.name, texAsset);
        }
        private static void LoadFontAsset(string path)
        {
            string assetName = DataUtil.GetCENameFromPath(path);

            if (_fonts.ContainsKey(assetName)) return;

            LoadFontFromData(path, File.ReadAllBytes(path));
        }
        private static void LoadSettingsAsset(string path)
        {
            string name = DataUtil.GetCENameFromPath(path);
            if (name == "CEConfig" || _settings.ContainsKey(name))
            {
                return;
            }

            Settings settings = new Settings();
            settings.LoadFile(path);
            SettingAsset asset = new SettingAsset(path, settings);

            _settings.Add(name, asset);
        }
        private static void LoadAudioAsset(string path)
        {
            string name = DataUtil.GetCENameFromPath(path);

            if (_audioClips.ContainsKey(name)) return;

            AudioClip? clip = AudioClip.Load(path);
            if (clip == null) return;
            AudioAsset audioAsset = new AudioAsset(path, clip);
            _audioClips.Add(name, audioAsset);
        }
        private static void LoadTextAsset(string path)
        {
            string name = DataUtil.GetCENameFromPath(path);

            if (_texts.ContainsKey(name)) return;

            byte[] textBytes = File.ReadAllBytes(path);
            Encoding encoding = DataUtil.DetectTextEncoding(path, out float confidence);
            if (confidence > 0 && confidence < 0.1f)
            {
                Debug.LogWarning($"Text encoding detection on low confidence in '{name}'. Detected '{encoding.EncodingName}'");
            }
            string text = encoding.GetString(textBytes);

            TextAsset textAsset = new TextAsset(path, text);
            _texts.Add(name, textAsset);
        }
        private static void LoadSceneAsset(string path)
        {
            string name = DataUtil.GetCENameFromPath(path);
            if (_scenes.ContainsKey(name)) return;

            byte[] textBytes = File.ReadAllBytes(path);
            Encoding encoding = DataUtil.DetectTextEncoding(path, out float confidence);
            if (confidence > 0 && confidence < 0.1f)
            {
                Debug.LogWarning($"Text encoding detection on low confidence in scene '{name}'. Detected '{encoding.EncodingName}'");
            }
            string text = encoding.GetString(textBytes);

            TextAsset textAsset = new TextAsset(path, text);
            _scenes.Add(name, textAsset);
        }

        private static AssetCompilationInfo CompileDataAssets()
        {
            float startTime = Time.GameTime;
            long rawTextureSize = UsageProfiler.TextureMemoryUsage;
            long rawAudioSize = UsageProfiler.AudioMmeoryUsage;
            long textureSize, audioSize, shaderSize, fontSize, settingsSize, textSize;

            // Compiling texture data
            string texturePath = Directories.RuntimeAssetsPath + "RuntimeTextures.rtmAsset";
            using (FileStream textureFileStream = File.Create(texturePath))
            {
                foreach (KeyValuePair<string, TextureAsset> pair in _textures.ToArray())
                {
                    MetaFile? meta = pair.Value.Meta;
                    bool? includeInRelease = pair.Value.Meta?.GetBoolProperty("IncludeInRelease");

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
                    bool? includeInRelease = meta?.GetBoolProperty("IncludeInRelease");

                    if (includeInRelease == null || includeInRelease.Value)
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
                    bool? includeInRelease = meta?.GetBoolProperty("IncludeInRelease");

                    if (includeInRelease == null || includeInRelease.Value)
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

                    if (includeInRelease == null || includeInRelease.Value)
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
                    settingsFileStream.Write(AssetCompiler.CompileData(pair.Value.name, Encoding.Unicode.GetBytes(DataUtil.XorString(pair.Value.settings.AsText))));
                }
                settingsSize = settingsFileStream.Length;
            }

            // Compile global settings
            byte[] globalSettingsData = AssetCompiler.CompileData("CEConfig", Encoding.Unicode.GetBytes(DataUtil.XorString(Settings.GlobalSettings.AsText, '%')));
            File.WriteAllBytes(Directories.RuntimeAssetsPath + "CEConfig.rtmAsset", globalSettingsData);
            settingsSize += globalSettingsData.Length;

            // Compile text assets
            string textPath = Directories.RuntimeAssetsPath + "RuntimeText.rtmAsset";
            using (FileStream textFileStream = File.Create(textPath))
            {
                foreach (KeyValuePair<string, TextAsset> pair in _texts.ToArray())
                {
                    textFileStream.Write(AssetCompiler.CompileTextAsset(pair.Value));
                }
                textSize = textFileStream.Length;
            }

            float compilationTime = Time.GameTime - startTime;

            return new AssetCompilationInfo(rawTextureSize, textureSize, rawAudioSize, audioSize, shaderSize, fontSize, settingsSize + textSize, compilationTime);
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

                string textpath = Directories.RuntimeAssetsPath + "RuntimeText.rtmAsset";
                if (File.Exists(textpath)) _texts = AssetCompiler.DecompileTextAssets(File.ReadAllBytes(textpath));
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
            if (!_precompiled) Debug.LogLocalInfo("Asset Loader", "Running on dynamic assets");

            float startTime = Time.GameTime;

            // Starts a recursive loop of searching directories in the "Assets" folder
            // Responsible for loading all dynamic assets
            AssetSearchDirectory(Directories.AssetsPath);

            float loadingTime = Time.GameTime - startTime;
            Debug.LogLocalInfo("Asset Loader", $"Dynamic assets loaded in {DataUtil.SecondsToTimeString(loadingTime)}");
        }

        private static void Compile()
        {
            Debug.WriteToConsole($"Precompiling all assets...", ConsoleColor.Blue);

            AssetCompiler.LoadCompilationSettings();

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
            _precompiled = true;

            float startTime = Time.GameTime;

            // Loads only precompiled assets
            if (!LoadPrecompiledAssets())
            {
                return;
            }

            float loadingTime = Time.GameTime - startTime;
            Debug.LogLocalInfo("Asset Loader", $"Precompiled assets loaded in {DataUtil.SecondsToTimeString(loadingTime)}");
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
