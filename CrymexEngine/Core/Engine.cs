using CrymexEngine.Data;
using CrymexEngine.Scenes;
using CrymexEngine.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text;

namespace CrymexEngine
{
    public static class Engine
    {
        public static string MainDirPath => _mainDirPath;
        public static readonly Version version = new Version(0, 0, 1);
        public static bool Initialized => _initialized;
        public static bool Precompiled => _precompiled;

        private static bool _initialized = false;
        private static string _mainDirPath;
        private static bool _precompiled;

        public static void Initialize(string? startupPath = null)
        {
            if (_initialized) return;

            if (startupPath == null)
            {
                startupPath = Directory.GetCurrentDirectory() + '\\';
            }
            _mainDirPath = startupPath;
            Directories.Init();

            GLFW.Init();

            Debug.InitializeEngineDirectories();

            bool settingsLoadedFine = LoadGlobalSettings();

            Debug.Instance.LoadSettings();

            if (!settingsLoadedFine)
            {
                Debug.LogError($"An error occured while loading precompiled settings. Running on dynamic assets");
            }

            Debug.LogLocalInfo("Engine", $"Crymex engine version {version.Major}.{version.Minor}.b{version.Build}");

            _initialized = true;
        }

        private static bool LoadGlobalSettings()
        {
            if (File.Exists(Directories.RuntimeAssetsPath + "CEConfig.rtmAsset"))
            {
                _precompiled = true;

                string cfgText;
                try
                {
                    cfgText = Encoding.Unicode.GetString(AssetCompiler.DecompileData(File.ReadAllBytes(Directories.RuntimeAssetsPath + "CEConfig.rtmAsset"), out _));
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

            Window.End();
        }

        public static void ErrorQuit(string errorMessage)
        {
            if (!_initialized) return;

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
            Audio.Cleanup();
            Assets.Cleanup();
            Debug.Instance.Cleanup();

            GLFW.Terminate();
        }
    }
}
