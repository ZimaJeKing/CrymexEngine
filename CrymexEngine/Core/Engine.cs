using CrymexEngine.Scenes;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CrymexEngine
{
    public static class Engine
    {
        public static readonly Version version = new Version(0, 0, 0, 0);

        private static unsafe void Main()
        {
            GLFW.Init();

            Settings.Instance.LoadSettings();

            Debug.Instance.Init();

            // --- Main application loop --- //
            Window.Instance.Run();

            // --- On program end --- //
            LogQuitDebugInfo();
            PerformCleanup();
        }

        public static void Quit()
        {
            Window.Instance.End();
        }

        public static void ErrorQuit(string errorMessage)
        {
            Debug.LogError(errorMessage);
            Quit();
        }

        private static void LogQuitDebugInfo()
        {
            Debug.LogStatus($"Ended after: {CEUtilities.SecondsToTimeString(Time.GameTime)}");
            Debug.LogStatus($"Loaded {Scene.Current.scriptableBehaviours.Count} behaviours and {Scene.Current.entities.Count + Scene.Current.uiElements.Count} game objects before quit");
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
