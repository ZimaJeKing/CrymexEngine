using CrymexEngine.Audio;
using CrymexEngine.Data;
using CrymexEngine.Scenes;
using CrymexEngine.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text;

namespace CrymexEngine
{
    public static class Engine
    {
        public static char PSep => Path.DirectorySeparatorChar;
        public static string MainDirPath => _mainDirPath;
        public static readonly Version version = new Version(0, 1, 1);
        public static bool Initialized => _initialized;
        public static bool Running => _running;
        public static bool SettingsPrecompiled => _precompiled;

        private static bool _initialized = false;
        private static string _mainDirPath;
        private static bool _precompiled;
        private static bool _running;

        public static void Initialize(string? startupPath = null)
        {
            if (_initialized) return;

            if (startupPath == null)
            {
                startupPath = Directory.GetCurrentDirectory() + PSep;
            }
            else if (!Directory.Exists(startupPath))
            {
                Debug.LogWarning($"Specified startup directory doesn't exist ('{startupPath}')");
                startupPath = Directory.GetCurrentDirectory() + PSep;
            }
            _mainDirPath = startupPath;

            Directories.Init();

            if (!GLFW.Init())
            {
                Debug.LogError("GLFW prohibited engine initialization.");
                return;
            }

            if (OperatingSystem.IsLinux()) Debug.InitializeUnixEngineDirectories();
            else Debug.InitializeEngineDirectories();

            bool settingsLoadedFine = LoadGlobalSettings();

            if (!settingsLoadedFine)
            {
                Debug.LogError($"An error occured while loading precompiled settings. Running on dynamic assets");
            }

            if (Settings.GlobalSettings.GetSetting("LogWorkingDirectory", out SettingOption pwdOption, SettingType.Bool) && pwdOption.GetValue<bool>())
            {
                Debug.LogLocalInfo("Engine", $"Working directory: '{_mainDirPath}'");
            }

            Debug.LoadSettings();

            DataUtil.LoadSettings();

            Debug.LogLocalInfo("Engine", $"Crymex engine version {version.Major}.{version.Minor}.b{version.Build}");

            _initialized = true;
        }

        private static bool LoadGlobalSettings()
        {
            string path = Directories.RuntimeAssetsPath + "CEConfig.rtmAsset";
            if (File.Exists(path))
            {
                _precompiled = true;

                string cfgText;
                try
                {
                    cfgText = DataUtil.XorString(Encoding.Unicode.GetString(AssetCompiler.DecompileData(File.ReadAllBytes(path), out _)));
                }
                catch
                {
                    _precompiled = false;
                    Settings.GlobalSettings.LoadFile(Directories.AssetsPath + "CEConfig.cfg");
                    return false;
                }

                // Load global config from precompiled file
                Settings.GlobalSettings.LoadText(cfgText);
            }
            else
            {
                Settings.GlobalSettings.LoadFile(Directories.AssetsPath + "CEConfig.cfg");
            }
            return true;
        }

        public static void Run()
        {
            if (!_initialized)
            {
                Debug.LogError("Engine wasn't initialized properly. Try calling Engine.Initialize() before running the engine.");
                return;
            }

            _running = true;

            // --- Main application loop --- //
            Window.Run();

            // --- On program end --- //
            LogQuitDebugInfo();
            PerformCleanup();
            _initialized = false;
        }

        public static void Quit()
        {
            if (!_initialized) return;

            _running = false;

            Window.End();
        }

        public static void ErrorQuit(string errorMessage)
        {
            if (!_initialized) return;

            _running = false;

            Debug.LogError(errorMessage);
            Quit();
        }

        private static void LogQuitDebugInfo()
        {
            Debug.LogLocalInfo("Engine", $"Ended after: {DataUtil.SecondsToTimeString(Time.GameTime)}");
            Debug.LogLocalInfo("Engine", $"Loaded {Scene.Current.scriptableBehaviours.Count} behaviours and {Scene.Current.entities.Count + Scene.Current.uiElements.Count} game objects before quit");
        }

        private static void PerformCleanup()
        {
            ALMgr.Cleanup();
            Assets.Cleanup();
            Debug.Cleanup();

            GLFW.Terminate();
        }
    }
}
