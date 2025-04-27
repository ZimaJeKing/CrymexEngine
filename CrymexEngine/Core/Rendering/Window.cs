using CrymexEngine.AetherPhysics;
using CrymexEngine.Audio;
using CrymexEngine.Data;
using CrymexEngine.Debugging;
using CrymexEngine.GameObjects;
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
    public static class Window
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
        private static Version _glVersion = new Version(3, 3);
        private static Vector2 _halfSize = new Vector2(128);
        private static WindowCursor? _windowCursor;
        private static Texture _windowIcon;
        private static WindowBorder _windowBorder = WindowBorder.Fixed;
        private static ContextFlags _glContextFlags = ContextFlags.Default;
        private static bool _logFPS;
        private static Vector2 _oneOverSize;
        private static Vector2 _oneOverHalfSize;
        private static bool _hasWayland = false;
        private static bool _tabX;

        public static GameWindow GLFWWindow => _glfwWindow;

        public static bool Focused => _glfwWindow.IsFocused;
        public static bool Loaded => _loaded;
        public static int FramesPerSecond => _framesPerSecond;
        public static Vector2 HalfSize => _halfSize;
        public static Vector2 OneOverSize => _oneOverSize;
        public static Vector2 OneOverHalfSize => _oneOverHalfSize;

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
            get => _title;
            set
            {
                _title = value;
                _glfwWindow.Title = value;
            }
        }
        public static unsafe bool Resizable
        {
            get => _resizable;
            set
            {
                GLFW.SetWindowAttrib(_glfwWindow.WindowPtr, WindowAttribute.Resizable, value);
                _resizable = value;
            }
        }
        public static Vector2i Size
        {
            get => _size;
            set
            {
                _size = value;
                _halfSize = value.ToVector2() * 0.5f;
                _oneOverSize = new Vector2(1f / _size.X, 1f / _size.Y);
                _oneOverHalfSize = _oneOverSize * 2;
                _glfwWindow.ClientSize = value;
            }
        }
        public static Texture Icon
        {
            get => _windowIcon;
            set
            {
                if (_hasWayland)
                {
                    Debug.LogError("Wayland doesn't support window icons. Please use X11 instead.");
                    return;
                }
                SetWindowIcon(value);
            }
        }
        public static WindowCursor? Cursor
        {
            get => _windowCursor;
            set
            {
                if (value == null)
                {
                    _glfwWindow.Cursor = MouseCursor.Default;
                    _windowCursor = null;
                    return;
                }

                Texture texture = value.texture.Resize(value.size.X, value.size.Y);

                texture.FlipY();

                _windowCursor = value;
                MouseCursor cursor = new MouseCursor(value.hotspot.X, value.hotspot.Y, value.size.X, value.size.Y, texture.GetRawData());
                _glfwWindow.Cursor = cursor;
            }
        }
        public static bool VSync
        {
            get => _vSync;
            set
            {
                _vSync = value;
                if (value) _glfwWindow.VSync = VSyncMode.On;
                else _glfwWindow.VSync = VSyncMode.Off;
            }
        }
        public static WindowState WindowState
        {
            get => _windowState;
            set
            {
                _glfwWindow.WindowState = value;
                _windowState = value;
            }
        }
        public static int MaxFPS
        {
            get => _maxFPS;
            set
            {
                if (value < 0) return; 
                _maxFPS = value;
            }
        }
        public static int MaxFPSIdle
        {
            get => _maxFPSIdle;
            set
            {
                if (value < 0) return;
                _maxFPSIdle = value;
            }
        }

        internal static void Run()
        {
            if (_loaded) return;

            GLFWProvider.SetErrorCallback((errorCode, description) =>
            {
                Debug.LogError($"GLFW Error ({errorCode}): {description}");
            });

            _windowIcon = Texture.None;

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

        internal static void End()
        {
            _glfwWindow?.Close();
        }

        private static unsafe void WindowLoad()
        {
            _tabX = Settings.GlobalSettings.GetSetting("TabXMinimize", out SettingOption tabXSetting, SettingType.Bool) && tabXSetting.GetValue<bool>();

            // Wayland on linux has only limited functionality
            _hasWayland = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE") == "wayland";

            _glfwWindow.IsVisible = true;

            // Wayland on linux lacks the functionality to position the window
            if (!_hasWayland) _glfwWindow.CenterWindow();
            else Debug.LogWarning("Limited compatibility with Wayland. Please use X11 instead.");

            // Create engine repeat events
            EventSystem.AddEventRepeat("CE_SecondLoop", SecondLoop, 1f, true);

            Camera.MainCamera.Init();
            Assets.LoadAssets();

            if (!Engine.Running) return;

            ApplyPostLoadSettings();

            // Initialize audio
            if (Settings.GlobalSettings.GetSetting("UseAudio", out SettingOption audioSettingOption, SettingType.Bool) && audioSettingOption.GetValue<bool>())
            {
                if (!ALMgr.InitializeContext())
                {
                    ALMgr.OverrideContext();
                }
            }

            Physics.Init();
            Input.Init();

            UsageProfiler.Init();

            _loaded = true;

            Debug.LogLocalInfo("Window", $"Window loaded in: {DataUtil.SecondsToTimeString(Time.GameTime)}");

            if (Settings.GlobalSettings.GetSetting("StartingScene", out SettingOption startingSceneSetting, SettingType.RefString))
            {
                string sceneName = startingSceneSetting.GetValue<string>();
                TextAsset? sceneAsset = Assets.GetSceneAsset(sceneName);
                if (sceneAsset != null)
                {
                    Debug.LogLocalInfo("Scene Loader", $"Running scene: '{sceneName}'");
                    SceneLoader.LoadSceneFromTextImmediate(sceneAsset.text);
                }
            }

            // Loads scriptable behaviours and calls Load
            ScriptLoader.LoadBehaviours();
        }

        private static void WindowUpdate(FrameEventArgs e)
        {
            if (!Engine.Running) return;

            UsageProfiler.BeginProcessorTimeQuery();

            _fpsCounter++;

            // Clearing the screen
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Time.Set((float)e.Time);

            if (!Input.Optimized) Input.Update();
            EventSystem.Instance.Update();

            TextEditor.Instance.Update();

            // Call Update on all loaded behaviours
            UpdateBehaviours();

            Input.textInput = string.Empty;

            HandleErrors();

            // Minimize the window with Tab + X
            if (_tabX && Input.Key(Key.Tab) && Input.Key(Key.X)) _glfwWindow.WindowState = WindowState.Minimized;

            _glfwWindow.SwapBuffers();

            UsageProfiler.EndProcessorTimeQuery();
        }

        private static void WindowResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _size = e.Size;
            _halfSize = e.Size.ToVector2() * 0.5f;
            _oneOverSize = new Vector2(1f / _size.X, 1f / _size.Y);
            _oneOverHalfSize = _oneOverSize * 2;
        }

        private static void WindowQuit(CancelEventArgs e)
        {
            foreach (ScriptableBehaviour behaviour in Scene.Current.scriptableBehaviours)
            {
                Behaviour.QuitBehaviour(behaviour);
            }
            foreach (Entity entity in Scene.Current.entities)
            {
                GameObject.GameObjectQuit(entity);
            }
            foreach (UIElement element in Scene.Current.uiElementDeleteQueue)
            {
                GameObject.GameObjectQuit(element);
            }
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
            if (_logFPS) Debug.WriteToConsole("FPS: " + _fpsCounter, ConsoleColor.DarkGreen);
            _framesPerSecond = _fpsCounter;
            _fpsCounter = 0;

            UsageProfiler.UpdateStats();

            ALMgr.RemoveInactiveSources();
        }

        private static void HandleErrors()
        {
            GLErrorCode glError = GL.GetError();
            if (glError != GLErrorCode.NoError)
            {
                Debug.LogError("OpenGL Error: " + glError);
            }

            if (ALMgr.Initialized)
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
            for (int i = 0; i < Scene.Current.scriptableBehaviours.Count; i++)
            {
                ScriptableBehaviour behaviour = Scene.Current.scriptableBehaviours[i];
                if (behaviour.enabled) Behaviour.UpdateBehaviour(behaviour);
            }

            // Update entities
            for (int i = 0; i < Scene.Current.entities.Count; i++)
            {
                Entity entity = Scene.Current.entities[i];
                if (entity.enabled) GameObject.GameObjectUpdate(entity);
            }

            // Render world space lines
            for (int i = 0; i < Scene.Current.lines.Count; i++)
            {
                LineGroup line = Scene.Current.lines[i];
                if (line.enabled && !line.ScreenSpace) Behaviour.UpdateBehaviour(line);
            }

            // Update UI elements and configure transparency
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);

            // Render elemets
            for (int i = 0; i < Scene.Current.uiElements.Count; i++)
            {
                UIElement element = Scene.Current.uiElements[i];
                if (element.enabled) GameObject.GameObjectUpdate(element);
            }

            // Render text
            for (int i = 0; i < Scene.Current.textObjects.Count; i++)
            {
                TextObject textObject = Scene.Current.textObjects[i];
                if (textObject.enabled) TextObject.RenderText(textObject);
            }

            // Render screen space lines
            for (int i = 0; i < Scene.Current.lines.Count; i++)
            {
                LineGroup line = Scene.Current.lines[i];
                if (line.enabled && line.ScreenSpace) Behaviour.UpdateBehaviour(line);
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
            // If the window should log fps every frame
            if (Settings.GlobalSettings.GetSetting("LogFPS", out SettingOption logFPSOption, SettingType.Bool) && logFPSOption.GetValue<bool>())
            {
                _logFPS = true;
            }

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
                Vector2i version = VectorUtil.RoundToInt(glVersionSetting.GetValue<Vector2>());
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
                _oneOverSize = new Vector2(1f / _size.X, 1f / _size.Y);
                _oneOverHalfSize = _oneOverSize * 2;
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
            else Icon = Texture.White;

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

                Texture? texture = Assets.GetTexture(cursorTexSetting.GetValue<string>());
                if (texture == null)
                {
                    Cursor = null;
                    return;
                }
                Cursor = new WindowCursor(texture, size, hotspot);
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
            if (texture == null) return;

            Texture windowIcon = texture.Resize(32, 32);

            _windowIcon = windowIcon.Clone();

            windowIcon.FlipY();

            OpenTK.Windowing.Common.Input.Image img = new OpenTK.Windowing.Common.Input.Image(32, 32, windowIcon.GetRawData());
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

                NumberOfSamples = Camera.msaaSamples,
                DepthBits = 24,
                AutoLoadBindings = true,
                WindowBorder = _windowBorder,

                StartFocused = true,
                IsEventDriven = false
            };
        }
    }
}
