using CrymexEngine.Data;
using CrymexEngine.Scenes;
using CrymexEngine.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CrymexEngine
{
    public static class Engine
    {
        public static readonly Version version = new Version(0, 0, 1);
        public static bool Initialized => _initialized;

        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            GLFW.Init();

            Debug.InitializeEngineDirectories();

            Settings.GlobalSettings.LoadFile(Directories.assetsPath + "GlobalSettings.txt");

            Debug.Instance.LoadSettings();

            Debug.LogLocalInfo("Engine", $"Crymex engine version {version.Major}.{version.Minor}.b{version.Build}");

            _initialized = true;
        }

        public static void Run()
        {
            // --- Main application loop --- //
            Window.Instance.Run();

            // --- On program end --- //
            LogQuitDebugInfo();
            PerformCleanup();
            _initialized = false;
        }

        public static void Quit()
        {
            if (!_initialized) return;

            Window.Instance.End();
        }

        public static void ErrorQuit(string errorMessage)
        {
            if (!_initialized) return;

            Debug.LogError(errorMessage);
            Quit();
        }

        private static void LogQuitDebugInfo()
        {
            Debug.LogLocalInfo("Engine", $"Ended after: {DataUtilities.SecondsToTimeString(Time.GameTime)}");
            Debug.LogLocalInfo("Engine", $"Loaded {Scene.Current.scriptableBehaviours.Count} behaviours and {Scene.Current.entities.Count + Scene.Current.uiElements.Count} game objects before quit");
        }

        private static void PerformCleanup()
        {
            Audio.Instance.Cleanup();
            Assets.Cleanup();
            Debug.Instance.Cleanup();

            GLFW.Terminate();
        }
    }
}
