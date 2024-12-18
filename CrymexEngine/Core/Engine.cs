using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CrymexEngine
{
    public static class Engine
    {
        public static string[] StartingArgs
        {
            get
            {
                return _startingArgs;
            }
        }

        private static string[] _startingArgs;

        private static unsafe void Main(string[] args)
        {
            _startingArgs = args;

            GLFW.Init();

            Settings.Instance.LoadSettings();

            Debug.Instance.Init();

            // --- Main application loop --- //
            Window.Run();

            // --- On program end --- //
            LogQuitDebugInfo();
            PerformCleanup();
        }

        public static void Quit()
        {
            Window.End();
        }

        public static void ErrorQuit(string errorMessage)
        {
            Debug.LogError(errorMessage);
            Quit();
        }

        private static void LogQuitDebugInfo()
        {
            Debug.LogStatus($"Ended after {Debug.FloatToShortString(Time.GameTime)} seconds");
            Debug.LogStatus($"Loaded {Scene.current.scriptableBehaviours.Count} behaviours and {Scene.current.entities.Count + Scene.current.uiElements.Count} game objects before quit");
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
