using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CrymexEngine
{
    public static class Engine
    {
        public static bool debugMode = false;

        private static unsafe void Main(string[] args)
        {
            GLFW.Init();

            Settings.LoadSettings();
            Debug.Init();

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
            Debug.LogStatus($"Ended after {Debug.DoubleToShortString(Time.GameTime)} seconds");
            Debug.LogStatus($"Loaded {Scene.current.scriptableBehaviours.Count} behaviours and {Scene.current.entities.Count + Scene.current.uiElements.Count} game objects before quit");
        }

        private static void PerformCleanup()
        {
            Audio.Cleanup();
            Assets.Cleanup();
            Debug.Cleanup();

            GLFW.Terminate();
        }
    }
}
