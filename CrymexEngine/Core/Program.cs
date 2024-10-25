using CrymexEngine.Rendering;
using CrymexEngine.Scripts;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace CrymexEngine
{
    public static class Program
    {
        public static GameWindow window;

        public static float deltaTime;
        public static float gameTime;
        public static bool debugMode;
        public static bool initialized { get; private set; } = false;
        public static unsafe bool windowResizeable
        {
            get
            {
                return _windowResizeable;
            }
            set
            {
                GLFW.SetWindowAttrib(window.WindowPtr, WindowAttribute.Resizable, value);
            }
        }
        public static Vector2i windowSize
        {
            get
            {
                return _windowSize;
            }
            set
            {
                _windowSize = value;
                window.Size = value;
            }
        }
        public static Vector2i monitorResolution { get; private set; }
        public static Texture windowIcon
        {
            set
            {
                Texture winIcon = value.Clone();
                winIcon.FlipY();
                window.Icon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(winIcon.width, winIcon.height, winIcon.data));
            }
        }
        public static WindowState windowState
        {
            get
            {
                return (WindowState)window.WindowState;
            }
            set
            {
                window.WindowState = (OpenTK.Windowing.Common.WindowState)value;
            }
        }
        public static int fps { get; private set; }

        private static Vector2i _windowSize;
        private static bool _windowResizeable = true;

        private static unsafe void Main(string[] args)
        {
            GLFW.Init();

            Assets.LoadSettings();

            // Create the Window
            window = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);

            // Window Events
            window.Load += Load;
            window.UpdateFrame += Update;
            window.Resize += Resize;
            window.Closing += Quit;
            window.TextInput += TextInput;

            // Running
            window.Run();
        }

        private unsafe static void Load()
        {
            GLFW.GetMonitorWorkarea((Monitor*)window.CurrentMonitor.Pointer, out int xPos, out int yPos, out int width, out int height);
            monitorResolution = new Vector2i(width, height);

            Shader.LoadDefaultShaders();
            Mesh.InitShapes();
            Assets.LoadTextures();
            BehaviourLoader.LoadBehaviours();

            Assets.ApplyPostSettings();

            for (int i = 0; i < Scene.behaviours.Count; i++)
            {
                Scene.behaviours[i].Load();
            }

            initialized = true;
        }

        private static int _fpsCounter;
        private static void Update(FrameEventArgs e)
        {
            // Time and FPS calculation
            deltaTime = (float)e.Time;
            gameTime += deltaTime;

            _fpsCounter++;
            tickLoopTimer += deltaTime;

            // TickLoop
            if (tickLoopTimer > 1) TickLoop();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            for (int i = 0; i < Scene.behaviours.Count; i++)
            {
                Scene.behaviours[i].Update();
            }
            for (int i = 0; i < Scene.entities.Count; i++)
            {
                Scene.entities[i].Update();
            }

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Debug.LogL("OpenGL Error:\n" + error, ConsoleColor.DarkRed);
            }

            if (Input.Key(Key.Tab) && Input.Key(Key.X)) window.WindowState = (OpenTK.Windowing.Common.WindowState)WindowState.Minimized;

            window.SwapBuffers();
        }

        private static float tickLoopTimer;
        private static void TickLoop()
        {
            tickLoopTimer -= 1;
            fps = _fpsCounter;
            _fpsCounter = 0;
            if (debugMode) Debug.LogL(fps, ConsoleColor.DarkGreen);
        }

        private static void Resize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _windowSize = e.Size;
        }

        private static void Quit(CancelEventArgs args)
        {
            Debug.LogL($"Loaded {Scene.behaviours.Count} behaviours", ConsoleColor.DarkYellow);
            Debug.LogL($"Loaded {Scene.entities.Count} entities", ConsoleColor.DarkYellow);
        }

        public static void Quit()
        {
            window.Close();
            GLFW.Terminate();
        }
        public static void Quit(string errorMessage)
        {
            Debug.LogL(errorMessage, ConsoleColor.DarkRed);
            Quit();
        }

        private static void TextInput(TextInputEventArgs e)
        {
            Input.textInput = e.AsString;
        }
    }

    public enum WindowState
    {
        //
        // Summary:
        //     The window is in its normal state.
        Normal,
        //
        // Summary:
        //     The window is minimized to the taskbar (also known as 'iconified').
        Minimized,
        //
        // Summary:
        //     The window covers the whole working area, which includes the desktop but not
        //     the taskbar and/or panels.
        Maximized,
        //
        // Summary:
        //     The window covers the whole screen, including all taskbars and/or panels.
        //     VSync stops working for some odd reason
        Fullscreen
    }
}
