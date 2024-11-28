using CrymexEngine.Scenes;
using CrymexEngine.Scripts;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;
using GLFWErrorCode = OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace CrymexEngine
{
    public static class Window
    {
        public static GameWindow glWindow { get; private set; }
        public static unsafe bool Resizable
        {
            get
            {
                return _resizable;
            }
            set
            {
                GLFW.SetWindowAttrib(glWindow.WindowPtr, WindowAttribute.Resizable, value);
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
                glWindow.Size = value;
            }
        }
        public static Texture Icon
        {
            set
            {
                if (value == null) return;
                Texture winIcon = value.Clone();
                winIcon.FlipY();
                glWindow.Icon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(winIcon.width, winIcon.height, winIcon.data));
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
                glWindow.Title = value;
            }
        }
        public static WindowState windowState { get; private set; }
        public static float deltaTime { get; private set; }
        public static float gameTime { get; private set; }
        public static float tickPeriod;
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
                glWindow.UpdateFrequency = value;
            }
        }
        public static bool isLoaded { get; private set; }
        public static int framesPerSecond { get; private set; }

        private static string _title = "";
        private static Vector2i _size;
        private static bool _resizable;
        private static int _maxFPS;

        private static Vector2i monitorResolution;

        private static int fpsCounter;
        private static float tickLoopTimer;

        // Frame and depth buffers
        private static int framebuffer;
        private static int framebufferTexture;
        private static int depthbuffer;

        public static void Run()
        {
            ApplyPreLoadSettings();

            // Window settings
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings
            {
                NumberOfSamples = Camera.msaaSamples,
                DepthBits = 24
            };

            // --- Create the glWindow and load OpenGL bindings ---
            glWindow = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);

            // Configure transparency
            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            // Window events
            glWindow.Load += Load;
            glWindow.UpdateFrame += Update;
            glWindow.Resize += Resize;
            glWindow.TextInput += WindowTextInput;
            glWindow.Closing += WindowQuit;

            // Running
            glWindow.Run();
        }

        private static unsafe void Load()
        {
            GLFW.GetMonitorWorkarea((Monitor*)glWindow.CurrentMonitor.Pointer, out int xPos, out int yPos, out int width, out int height);
            monitorResolution = new Vector2i(width, height);

            // Initializing CrymexEngine components
            Camera.Init();
            Assets.LoadAssets();
            ApplyPostLoadSettings();
            Audio.Init();

            // Load the user specified scene, otherwise create a new scene
            if (!Settings.GetSetting("StartingScene", out SettingOption startingSceneSetting) || !SceneLoader.LoadScene(Assets.GetScenePath(startingSceneSetting.GetValue<string>())))
            {
                Scene.Current = new Scene();
            }

            BehaviourLoader.LoadBehaviours();

            for (int i = 0; i < Scene.Current.behaviours.Count; i++)
            {
                Scene.Current.behaviours[i].Load();
            }

            isLoaded = true;
        }

        private static void Update(FrameEventArgs e)
        {
            // Time and FPS calculation
            deltaTime = (float)e.Time;
            gameTime += deltaTime;

            fpsCounter++;
            tickLoopTimer += deltaTime;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // TickLoop
            if (tickLoopTimer > tickPeriod) TickLoop();

            foreach (Behaviour behaviour in Scene.Current.behaviours)
            {
                behaviour.Update();
            }
            foreach (Entity entity in Scene.Current.entities)
            {
                entity.Update();
            }

            HandleErrors();

            if (Input.Key(Key.Tab) && Input.Key(Key.X)) glWindow.WindowState = (OpenTK.Windowing.Common.WindowState)WindowState.Minimized;

            Audio.Update();

            glWindow.SwapBuffers();
        }

        private static void TickLoop()
        {
            tickLoopTimer -= 1;
            framesPerSecond = fpsCounter;
            fpsCounter = 0;

            UsageProfiler.UpdateStats();

            foreach (Behaviour behaviour in Scene.Current.behaviours)
            {
                behaviour.TickLoop();
            }
        }

        private static void Resize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _size = e.Size;
        }

        public static void End()
        {
            glWindow.Close();
        }

        private static void WindowQuit(CancelEventArgs e)
        {

        }

        private static void WindowTextInput(TextInputEventArgs e)
        {
            Input.textInput = e.AsString;
        }

        private static void ApplyPreLoadSettings()
        {
            // Tick period
            if (Settings.GetSetting("TickPeriod", out SettingOption tickPeriodSetting))
            {
                tickPeriod = tickPeriodSetting.GetValue<float>();
            }

            // PreRender Distance
            if (Settings.GetSetting("RenderDistance", out SettingOption renderDistSetting))
            {
                Camera.renderDistance = renderDistSetting.GetValue<float>();
            }

            // Debug Mode
            if (Settings.GetSetting("DebugMode", out SettingOption debugModeSetting))
            {
                Application.debugMode = debugModeSetting.GetValue<bool>();
            }
        }
        private static void ApplyPostLoadSettings()
        {
            // Window Title
            if (Settings.GetSetting("WindowTitle", out SettingOption windowTitleSetting))
            {
                Title = windowTitleSetting.GetValue<string>();
            }

            // Window resizability
            if (Settings.GetSetting("WindowResizable", out SettingOption windowResizableSetting))
            {
                Resizable = windowResizableSetting.GetValue<bool>();
            }

            // Window icon
            if (Settings.GetSetting("WindowIcon", out SettingOption windowIconSetting))
            {
                Icon = Assets.GetTexture(windowIconSetting.GetValue<string>());
            }

            // Window Size
            if (Settings.GetSetting("WindowSize", out SettingOption windowSizeSetting))
            {
                Size = (Vector2i)windowSizeSetting.GetValue<Vector2>();
            }

            // Max FPS
            if (Settings.GetSetting("MaxFPS", out SettingOption maxFPSSetting))
            {
                MaxFPS = maxFPSSetting.GetValue<int>();
            }

            // VSync
            if (Settings.GetSetting("VSync", out SettingOption vsyncSetting))
            {
                bool value = vsyncSetting.GetValue<bool>();
                if (value)
                {
                    glWindow.VSync = VSyncMode.On;
                }
                else
                {
                    glWindow.VSync = VSyncMode.Off;
                }
            }
        }

        private static void InitFramebuffer()
        {
            // Framebuffer creation and binding
            framebuffer = GL.GenBuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            framebufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Size.X, Size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, framebufferTexture, 0);

            // Depthbuffer creation and binding
            depthbuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthbuffer);

            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Size.X, Size.Y);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthbuffer);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private static void HandleErrors()
        {
            ErrorCode glError = GL.GetError();
            if (glError != ErrorCode.NoError)
            {
                Debug.LogError("OpenGL Error: " + glError);
            }

            ALError alError = AL.GetError();
            if (alError != ALError.NoError)
            {
                Debug.LogError("OpenAL Error: " + alError);
            }
        }
    }

    public enum WindowState
    {
        //
        // Summary:
        //     The glWindow is in its normal state.
        Normal,
        //
        // Summary:
        //     The glWindow is minimized to the taskbar (also known as 'iconified').
        Minimized,
        //
        // Summary:
        //     The glWindow covers the whole working area, which includes the desktop but not
        //     the taskbar and/or panels.
        Maximized,
        //
        // Summary:
        //     The glWindow covers the whole screen, including all taskbars and/or panels.
        //     VSync stops working for some odd reason
        Fullscreen
    }
}
