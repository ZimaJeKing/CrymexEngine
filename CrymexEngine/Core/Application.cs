using CrymexEngine.Rendering;
using CrymexEngine.Scenes;
using CrymexEngine.Scripting;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CrymexEngine
{
    public static class Application
    {
        public static bool debugMode = true;

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
            Debug.LogStatus($"Loaded {Scene.current.behaviours.Count} behaviours and {Scene.current.entities.Count} entities before quit");
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
