using CrymexEngine.Rendering;
using CrymexEngine.Scenes;
using CrymexEngine.Scripts;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using GLFWWindow = OpenTK.Windowing.GraphicsLibraryFramework;
using Window = CrymexEngine.Window;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

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
            UsageProfiler.Init();

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
            if (!debugMode) return;
            
            Debug.LogStatus($"Loaded {Scene.Current.behaviours.Count} behaviours and {Scene.Current.entities.Count} entities before quit");
        }

        private static void PerformCleanup()
        {
            Scene.Current.Clear();

            Audio.Cleanup();
            Assets.Cleanup();
            Debug.Cleanup();

            GLFW.Terminate();
        }
    }
}
