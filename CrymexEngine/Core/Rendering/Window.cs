using CrymexEngine.Debugging;
using CrymexEngine.Scenes;
using CrymexEngine.Scripting;
using CrymexEngine.UI;
using CrymexEngine.Utils;
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

namespace CrymexEngine
{
    public class Window
    {

        private static GameWindow _glfwWindow;
        private static WindowState _windowState;
        private static string _title = "";
        private static Vector2i _size = new Vector2i(256);
        private static bool _resizable;
        private static bool _vSync;
        private static int _maxFPS;
        private static int _maxFPSIdle;
        private static int _framesPerSecond = 0;
        private static int _fpsCounter = 0;
        private static bool _loaded;
        private static Version _glVersion = new Version(4, 5);
        private static Vector2 _halfSize = new Vector2(128);
        private static WindowCursor _windowCursor;
        private static Texture _windowIcon;
        private static WindowBorder _windowBorder = WindowBorder.Fixed;
        private static ContextFlags _glContextFlags = ContextFlags.Default;

        private static readonly Window _instance = new Window();

        /// <summary>
        /// An internal instance
        /// </summary>
        public static Window Instance => _instance;
        public GameWindow GLFWWindow => _glfwWindow;

        public static bool Loaded => _loaded;
        public static int FramesPerSecond => _framesPerSecond;
        public static Vector2 HalfSize => _halfSize;

        public static bool HideWindowBorder
        {
            get
            {
                if (_windowBorder == WindowBorder.Hidden) return true;
                else return false;
            }
            set
            {
                if (value)
                {
                    _windowBorder = WindowBorder.Hidden;
                }
                else
                {
                    _windowBorder = WindowBorder.Fixed;
                }
                _glfwWindow.WindowBorder = _windowBorder;
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
                _glfwWindow.Title = value;
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
                GLFW.SetWindowAttrib(_glfwWindow.WindowPtr, WindowAttribute.Resizable, value);
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
                _glfwWindow.ClientSize = value;
            }
        }
        public static Texture Icon
        {
            get
            {
                return _windowIcon;
            }
            set
            {
                SetWindowIcon(value);
            }
        }
        public static WindowCursor Cursor
        {
            get
            {
                return _windowCursor;
            }
            set
            {
                Texture texture = value.texture.Resize(value.size.X, value.size.Y);

                texture.FlipY();

                _windowCursor = value;
                MouseCursor cursor = new MouseCursor(value.hotspot.X, value.hotspot.Y, value.size.X, value.size.Y, texture.data);
                _glfwWindow.Cursor = cursor;
            }
        }
        public static bool VSync
        {
            get
            {
                return _vSync;
            }
            set
            {
                _vSync = value;
                if (value) _glfwWindow.VSync = VSyncMode.On;
                else _glfwWindow.VSync = VSyncMode.Off;
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
                _glfwWindow.WindowState = value;
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
            }
        }
        public static int MaxFPSIdle
        {
            get
            {
                return _maxFPSIdle;
            }
            set
            {
                if (value < 0) return;
                _maxFPSIdle = value;
            }
        }

        public void Run()
        {
            if (_loaded) return;

            _windowIcon = Texture.White;

            ApplyPreLoadSettings();

            // --- Create the GLFWWindow and load OpenGL bindings ---
            _glfwWindow = new GameWindow(GameWindowSettings.Default, GetWindowSettings());

            InitOpenGL();

            // Window events
            _glfwWindow.Load += WindowLoad;
            _glfwWindow.UpdateFrame += WindowUpdate;
            _glfwWindow.Resize += WindowResize;
            _glfwWindow.TextInput += WindowTextInput;
            _glfwWindow.Closing += WindowQuit;
            _glfwWindow.FocusedChanged += WindowFocusChanged;

            // Running
            _glfwWindow.Run();
        }

        public void End()
        {
            _glfwWindow?.Close();
        }

        private static unsafe void WindowLoad()
        {
            _glfwWindow.IsVisible = true;

            _glfwWindow.CenterWindow();

            // Create engine repeat events
            EventSystem.AddEventRepeat("CE_SecondLoop", SecondLoop, 1f, true);

            // Initializing CrymexEngine components
            Camera.MainCamera.Init();
            Assets.LoadAssets();

            ApplyPostLoadSettings();

            // Initialize audio
            if (Settings.GlobalSettings.GetSetting("UseAudio", out SettingOption audioSettingOption, SettingType.Bool) && audioSettingOption.GetValue<bool>())
            {
                Audio.Instance.InitializeContext();
            }

            UsageProfiler.Instance.Init();

            // WindowLoad the user specified scene, otherwise create a new scene
            if (Settings.GlobalSettings.GetSetting("StartingScene", out SettingOption startingSceneSetting, SettingType.RefString)) SceneLoader.LoadScene(Assets.GetScenePath(startingSceneSetting.GetValue<string>()));

            Physics.Init();

            _loaded = true;

            // Loads scriptable behaviours and calls Load
            ScriptLoader.LoadBehaviours();
        }

        private static void WindowUpdate(FrameEventArgs e)
        {
            UsageProfiler.Instance.BeginProcessorTimeQuery();

            _fpsCounter++;

            // Clearing the screen
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Time.Instance.Set((float)e.Time);

            UICanvas.Instance.Update();
            EventSystem.Instance.Update();

            TextEditor.Instance.Update();

            // Call Update on all loaded behaviours
            UpdateBehaviours();

            Input.textInput = string.Empty;

            HandleErrors();

            // Minimize the window with Tab + X
            if (Input.Key(Key.Tab) && Input.Key(Key.X)) _glfwWindow.WindowState = WindowState.Minimized;

            _glfwWindow.SwapBuffers();

            UsageProfiler.Instance.EndProcessorTimeQuery();
        }

        private static void WindowResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _size = e.Size;
            _halfSize = e.Size.ToVector2() * 0.5f;
        }

        private static void WindowQuit(CancelEventArgs e)
        {

        }

        private static void WindowTextInput(TextInputEventArgs e)
        {
            Input.textInput = e.AsString;
        }

        private static void WindowFocusChanged(FocusedChangedEventArgs args)
        {
            TextEditor.Deselect();
            if (args.IsFocused) _glfwWindow.UpdateFrequency = MaxFPS;
            else _glfwWindow.UpdateFrequency = MaxFPSIdle;
        }

        /// <summary>
        /// Happens once every second
        /// </summary>
        private static void SecondLoop()
        {
            if (UsageProfiler.Active) Debug.WriteToConsole("FPS: " + _fpsCounter, ConsoleColor.DarkGreen);
            _framesPerSecond = _fpsCounter;
            _fpsCounter = 0;

            UsageProfiler.Instance.UpdateStats();

            Audio.Instance.RemoveInactiveSources();
        }

        private static void HandleErrors()
        {
            GLErrorCode glError = GL.GetError();
            if (glError != GLErrorCode.NoError)
            {
                Debug.LogError("OpenGL Error: " + glError);
            }

            if (Audio.Initialized)
            {
                ALError alError = AL.GetError();
                if (alError != ALError.NoError)
                {
                    Debug.LogError("OpenAL Error: " + AL.GetErrorString(alError));
                }
            }

            GLFWErrorCode glfwError = GLFW.GetError(out string glfwErrorMessage);
            if (glfwError != GLFWErrorCode.NoError)
            {
                Debug.LogError("GLFW Error: " + glfwErrorMessage);
            }
        }

        private static void UpdateBehaviours()
        {
            // Update scriptable behaviours
            foreach (ScriptableBehaviour behaviour in Scene.Current.scriptableBehaviours)
            {
                if (behaviour.enabled) Behaviour.UpdateBehaviour(behaviour);
            }

            // Update entities
            foreach (Entity entity in Scene.Current.entities)
            {
                if (entity.enabled) GameObject.GameObjectUpdate(entity);
            }

            // Update UI elements and configure transparency
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);

            // Render elemets
            foreach (UIElement element in Scene.Current.uiElements)
            {
                if (element.enabled) GameObject.GameObjectUpdate(element);
            }

            // Render Text
            foreach (TextObject textObject in Scene.Current.textObjects)
            {
                if (textObject.enabled) TextObject.RenderText(textObject);
            }

            // Render text cursor
            TextEditor.Instance.RenderCursor();

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);

            Scene.UpdateQueues();
        }

        private static void ApplyPreLoadSettings()
        {
            // GL context flags
            if (Settings.GlobalSettings.GetSetting("GLDebugOutput", out SettingOption glContextSetting, SettingType.Bool) && glContextSetting.GetValue<bool>())
            {
                _glContextFlags = ContextFlags.Debug;
            }

            // Hiding window border
            if (Settings.GlobalSettings.GetSetting("HideWindowBorder", out SettingOption windowBorderSetting, SettingType.Bool) && windowBorderSetting.GetValue<bool>())
            {
                _windowBorder = WindowBorder.Hidden;
            }

            // OpenGL version
            if (Settings.GlobalSettings.GetSetting("GLVersion", out SettingOption glVersionSetting, SettingType.Vector2))
            {
                Vector2i version = VectorUtility.RoundToInt(glVersionSetting.GetValue<Vector2>());
                _glVersion = new Version(version.X, version.Y);
            }

            // Render distance
            if (Settings.GlobalSettings.GetSetting("RenderDistance", out SettingOption renderDistSetting, SettingType.Float))
            {
                Camera.RenderDistance = renderDistSetting.GetValue<float>();
            }

            // Window title
            if (Settings.GlobalSettings.GetSetting("WindowTitle", out SettingOption windowTitleSetting, SettingType.String))
            {
                _title = windowTitleSetting.GetValue<string>();
            }

            // Window size
            if (Settings.GlobalSettings.GetSetting("WindowSize", out SettingOption windowSizeSetting, SettingType.Vector2))
            {
                _size = (Vector2i)windowSizeSetting.GetValue<Vector2>();
                _halfSize = _size.ToVector2() * 0.5f;
            }

            // VSync
            if (Settings.GlobalSettings.GetSetting("VSync", out SettingOption vsyncSetting, SettingType.Bool))
            {
                _vSync = vsyncSetting.GetValue<bool>();
            }

            // Cull back face
            if (Settings.GlobalSettings.GetSetting("CullFace", out SettingOption windowStateSetting, SettingType.Bool))
            {
                if (windowStateSetting.GetValue<bool>())
                {
                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(TriangleFace.Back);
                }
                else
                {
                    GL.Disable(EnableCap.CullFace);
                }
            }
        }
        private static void ApplyPostLoadSettings()
        {
            // GL debug context
            if (_glContextFlags == ContextFlags.Debug)
            {
                GL.Enable(EnableCap.DebugOutput);
                GL.Enable(EnableCap.DebugOutputSynchronous);
                GL.DebugMessageCallback(new DebugProc(Debug.LogGLDebugInfo), IntPtr.Zero);

                Debug.LogLocalInfo("Window", "Showing additional GL output");
            }

            // Window resizability
            if (Settings.GlobalSettings.GetSetting("WindowResizable", out SettingOption windowResizableSetting, SettingType.Bool))
            {
                Resizable = windowResizableSetting.GetValue<bool>();
            }

            // Window start fullscreen
            if (Settings.GlobalSettings.GetSetting("StartFullscreen", out SettingOption startFullscreenSetting, SettingType.Bool) && startFullscreenSetting.GetValue<bool>())
            {
                _glfwWindow.MakeFullscreen(_glfwWindow.CurrentMonitor.Handle);
            }

            // Window icon
            if (Settings.GlobalSettings.GetSetting("WindowIcon", out SettingOption windowIconSetting, SettingType.RefString))
            {
                Icon = Assets.GetTexture(windowIconSetting.GetValue<string>());
            }
            else Icon = Assets.GetTexture("WindowIcon");

            // Max FPS
            if (Settings.GlobalSettings.GetSetting("MaxFPS", out SettingOption maxFPSSetting, SettingType.Int))
            {
                MaxFPS = maxFPSSetting.GetValue<int>();
                _glfwWindow.UpdateFrequency = _maxFPS;
            }

            // Max FPS Idle
            if (Settings.GlobalSettings.GetSetting("MaxFPSIdle", out SettingOption maxFPSIdleSetting, SettingType.Int))
            {
                MaxFPSIdle = maxFPSIdleSetting.GetValue<int>();
            }

            // Cursor
            if (Settings.GlobalSettings.GetSetting("WindowCursor", out SettingOption cursorTexSetting, SettingType.RefString))
            {
                Vector2i hotspot = Vector2i.Zero;
                Vector2i size = new Vector2i(16, 16);

                if (Settings.GlobalSettings.GetSetting("WindowCursorHotspot", out SettingOption hotspotSetting, SettingType.Vector2))
                {
                    hotspot = (Vector2i)hotspotSetting.GetValue<Vector2>();
                }
                if (Settings.GlobalSettings.GetSetting("WindowCursorSize", out SettingOption sizeSetting, SettingType.Vector2))
                {
                    size = (Vector2i)sizeSetting.GetValue<Vector2>();
                }

                Cursor = new WindowCursor(Assets.GetTexture(cursorTexSetting.GetValue<string>()), size, hotspot);
            }
        }

        private static void InitOpenGL()
        {
            // Log version info
            GLFW.GetVersion(out int glfwMajor, out int glfwMinor, out int glfwRevision);
            Debug.LogLocalInfo("Window", $"GLFW Version: {glfwMajor}.{glfwMinor}.{glfwRevision}");
            Debug.LogLocalInfo("Window", $"OpenGL Version: {GL.GetString(StringName.Version)}");
            Debug.LogLocalInfo("Window", $"GLSL Version: {GL.GetString(StringName.ShadingLanguageVersion)}");

            GL.ActiveTexture(TextureUnit.Texture0);

            // Transparency
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Depth testing
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            // MSAA
            GL.Enable(EnableCap.Multisample);
        }

        private static void SetWindowIcon(Texture texture)
        {
            Texture windowIcon = texture.Resize(32, 32);

            _windowIcon = windowIcon.Clone();

            windowIcon.FlipY();

            OpenTK.Windowing.Common.Input.Image img = new OpenTK.Windowing.Common.Input.Image(32, 32, windowIcon.data);
            _glfwWindow.Icon = new WindowIcon(img);
        }

        private static NativeWindowSettings GetWindowSettings()
        {
            VSyncMode vsyncMode = VSyncMode.Off;
            if (_vSync) vsyncMode = VSyncMode.On;

            return new NativeWindowSettings
            {
                Title = _title,
                ClientSize = _size,
                WindowState = _windowState,
                StartVisible = false,
                Vsync = vsyncMode,

                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = _glVersion,
                Flags = _glContextFlags,

                NumberOfSamples = 8,
                DepthBits = 24,
                AutoLoadBindings = true,
                WindowBorder = _windowBorder,

                StartFocused = true,
                IsEventDriven = false
            };
        }
    }
}
