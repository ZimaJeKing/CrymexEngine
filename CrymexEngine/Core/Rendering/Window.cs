using CrymexEngine.Debugging;
using CrymexEngine.Scenes;
using CrymexEngine.Scripting;
using CrymexEngine.UI;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel;
using GLErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;
using GLFWErrorCode = OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace CrymexEngine
{
    public static class Window
    {
        public static GameWindow GLFWWindow
        {
            get
            {
                return _glfwWindow;
            }
            private set
            {
                _glfwWindow = value;
            }
        }
        public static unsafe bool Resizable
        {
            get
            {
                return _resizable;
            }
            set
            {
                GLFW.SetWindowAttrib(GLFWWindow.WindowPtr, WindowAttribute.Resizable, value);
                _resizable = value;
            }
        }
        public static Vector2i Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                _halfSize = value.ToVector2() * 0.5f;
                GLFWWindow.ClientSize = value;
            }
        }
        public static Texture Icon
        {
            set
            {
                if (value == null) return;
                Texture winIcon = value.Clone();
                winIcon.FlipY();
                GLFWWindow.Icon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(winIcon.width, winIcon.height, winIcon.data));
            }
        }
        public static string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                GLFWWindow.Title = value;
            }
        }
        public static WindowState WindowState
        {
            get
            {
                return _windowState;
            }
            set
            {
                _windowState = value;
            }
        }
        public static int MaxFPS
        {
            get
            {
                return _maxFPS;
            }
            set
            {
                if (value < 0) return; 
                _maxFPS = value;
                GLFWWindow.UpdateFrequency = value;
            }
        }
        public static bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }
        }
        public static int FramesPerSecond
        {
            get
            {
                return _framesPerSecond;
            }
        }
        public static Vector2 HalfSize
        {
            get
            {
                return _halfSize;
            }
        }

        private static GameWindow _glfwWindow;
        private static WindowState _windowState;
        private static string _title = "";
        private static Vector2i _size;
        private static bool _resizable;
        private static int _maxFPS;
        private static int _framesPerSecond = 0;
        private static bool _isLoaded;
        private static Vector2 _halfSize;
        private static Vector2i _monitorResolution;

        public static void Run()
        {
            ApplyPreLoadSettings();

            // Window settings
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings
            {
                NumberOfSamples = Camera.msaaSamples,
                DepthBits = 24
            };

            // --- Create the GLFWWindow and load OpenGL bindings ---
            GLFWWindow = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);

            // Configure transparency
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Enable(EnableCap.Multisample);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            // Window events
            GLFWWindow.Load += Load;
            GLFWWindow.UpdateFrame += Update;
            GLFWWindow.Resize += Resize;
            GLFWWindow.TextInput += WindowTextInput;
            GLFWWindow.Closing += WindowQuit;

            // Running
            GLFWWindow.Run();
        }
        public static void End()
        {
            GLFWWindow.Close();
        }

        private static unsafe void Load()
        {
            GLFW.GetMonitorWorkarea((Monitor*)GLFWWindow.CurrentMonitor.Pointer, out int xPos, out int yPos, out int width, out int height);
            _monitorResolution = new Vector2i(width, height);

            // Initializing CrymexEngine components
            Camera.Init();
            Assets.LoadAssets();
            ApplyPostLoadSettings();
            Audio.Init();

            UsageProfiler.Init();

            // Load the user specified scene, otherwise create a new scene
            if (!Settings.GetSetting("StartingScene", out SettingOption startingSceneSetting, SettingType.RefString) || !SceneLoader.LoadScene(Assets.GetScenePath(startingSceneSetting.GetValue<string>())))
            {
                Scene.current = new Scene();
            }

            // Loads behaviours and calls Load
            BehaviourLoader.LoadBehaviours();

            _isLoaded = true;
        }

        private static void Update(FrameEventArgs e)
        {
            UsageProfiler.BeginProcessorTimeQuery();

            // Clearing the screen
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Time and FPS calculation
            Time.Set((float)e.Time);

            // Updating behaviours and entities
            foreach (Behaviour behaviour in Scene.current.behaviours)
            {
                behaviour.Update();
            }
            foreach (Entity entity in Scene.current.entities)
            {
                entity.Update();
            }

            UICanvas.Update();

            HandleErrors();

            if (Input.Key(Key.Tab) && Input.Key(Key.X)) GLFWWindow.WindowState = (OpenTK.Windowing.Common.WindowState)WindowState.Minimized;

            Audio.Update();

            GLFWWindow.SwapBuffers();

            UsageProfiler.EndProcessorTimeQuery();
        }

        private static void Resize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _size = e.Size;
        }

        private static void WindowQuit(CancelEventArgs e)
        {

        }

        private static void WindowTextInput(TextInputEventArgs e)
        {
            Input.textInput = e.AsString;
        }

        private static void HandleErrors()
        {
            GLErrorCode glError = GL.GetError();
            if (glError != GLErrorCode.NoError)
            {
                Debug.LogError("OpenGL Error: " + glError);
            }

            ALError alError = AL.GetError();
            if (alError != ALError.NoError)
            {
                Debug.LogError("OpenAL Error: " + AL.GetErrorString(alError));
            }

            GLFWErrorCode glfwError = GLFW.GetError(out string glfwErrorMessage);
            if (glfwError != GLFWErrorCode.NoError)
            {
                Debug.LogError("GLFW Error: " + glfwErrorMessage);
            }
        }

        private static void ApplyPreLoadSettings()
        {
            // PreRender Distance
            if (Settings.GetSetting("RenderDistance", out SettingOption renderDistSetting, SettingType.Float))
            {
                Camera.RenderDistance = renderDistSetting.GetValue<float>();
            }

            // Debug Mode
            if (Settings.GetSetting("DebugMode", out SettingOption debugModeSetting, SettingType.Bool))
            {
                Application.debugMode = debugModeSetting.GetValue<bool>();
            }
        }
        private static void ApplyPostLoadSettings()
        {
            // Window Title
            if (Settings.GetSetting("WindowTitle", out SettingOption windowTitleSetting, SettingType.String))
            {
                Title = windowTitleSetting.GetValue<string>();
            }

            // Window resizability
            if (Settings.GetSetting("WindowResizable", out SettingOption windowResizableSetting, SettingType.Bool))
            {
                Resizable = windowResizableSetting.GetValue<bool>();
            }

            // Window icon
            if (Settings.GetSetting("WindowIcon", out SettingOption windowIconSetting, SettingType.RefString))
            {
                Icon = Assets.GetTexture(windowIconSetting.GetValue<string>());
            }

            // Window Size
            if (Settings.GetSetting("WindowSize", out SettingOption windowSizeSetting, SettingType.Vector2))
            {
                Size = (Vector2i)windowSizeSetting.GetValue<Vector2>();
            }

            // Max FPS
            if (Settings.GetSetting("MaxFPS", out SettingOption maxFPSSetting, SettingType.Int))
            {
                MaxFPS = maxFPSSetting.GetValue<int>();
            }

            // VSync
            if (Settings.GetSetting("VSync", out SettingOption vsyncSetting, SettingType.Bool))
            {
                if (vsyncSetting.GetValue<bool>())
                {
                    GLFWWindow.VSync = VSyncMode.On;
                }
                else
                {
                    GLFWWindow.VSync = VSyncMode.Off;
                }
            }
        }
    }

    public enum WindowState
    {
        //
        // Summary:
        //     The GLFWWindow is in its normal state.
        Normal,
        //
        // Summary:
        //     The GLFWWindow is minimized to the taskbar (also known as 'iconified').
        Minimized,
        //
        // Summary:
        //     The GLFWWindow covers the whole working area, which includes the desktop but not
        //     the taskbar and/or panels.
        Maximized,
        //
        // Summary:
        //     The GLFWWindow covers the whole screen, including all taskbars and/or panels.
        //     VSync stops working for some odd reason
        Fullscreen
    }
}
