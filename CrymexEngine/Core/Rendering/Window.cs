﻿using CrymexEngine.Debugging;
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

namespace CrymexEngine
{
    public class Window
    {
        /// <summary>
        /// An internal instance
        /// </summary>
        public static Window Instance
        {
            get
            {
                return _instance;
            }
        }

        public static GameWindow GLFWWindow
        {
            get
            {
                return _glfwWindow;
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
            get
            {
                return _windowIcon;
            }
            set
            {
                if (value == null) return;

                Texture texture = value.Resize(32, 32);

                _windowIcon = texture.Clone();

                texture.FlipY();

                OpenTK.Windowing.Common.Input.Image img = new OpenTK.Windowing.Common.Input.Image(32, 32, texture.data);
                GLFWWindow.Icon = new WindowIcon(img);
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
                GLFWWindow.Cursor = cursor;
            }
        }
        public static VSyncMode VSyncMode
        {
            get
            {
                return _vSyncMode;
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
        private static VSyncMode _vSyncMode;
        private static string _title = "";
        private static Vector2i _size;
        private static bool _resizable;
        private static int _maxFPS;
        private static int _maxFPSIdle;
        private static int _framesPerSecond = 0;
        private static int _fpsCounter = 0;
        private static bool _isLoaded;
        private static Vector2 _halfSize;
        private static WindowCursor _windowCursor;
        private static Texture _windowIcon;

        private static Window _instance = new Window();

        public static void Run()
        {
            if (_isLoaded) return;

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

        public static void End()
        {
            GLFWWindow.Close();
        }

        private static unsafe void WindowLoad()
        {
            _glfwWindow.IsVisible = true;

            _glfwWindow.CenterWindow();

            EventSystem.AddEventRepeat("CE_SecondLoop", new Action(SecondLoop), 1f);

            // Initializing CrymexEngine components
            Camera.Instance.Init();
            Assets.LoadAssets();

            ApplyPostLoadSettings();

            // Initialize audio
            if (Settings.GetSetting("UseAudio", out SettingOption audioSettingOption, SettingType.Bool) && audioSettingOption.GetValue<bool>())
            {
                Audio.Instance.Init();
            }

            UsageProfiler.Instance.Init();

            // WindowLoad the user specified scene, otherwise create a new scene
            if (!Settings.GetSetting("StartingScene", out SettingOption startingSceneSetting, SettingType.RefString) || !SceneLoader.LoadScene(Assets.GetScenePath(startingSceneSetting.GetValue<string>())))
            {
                Scene.current = new Scene();
            }

            // Loads scriptableBehaviours and calls Load
            ScriptLoader.LoadBehaviours();

            _isLoaded = true;
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

            // Call Update on all loaded behaviours
            UpdateBehaviours();

            HandleErrors();

            // Minimize the window with Tab + X
            if (Input.Key(Key.Tab) && Input.Key(Key.X)) _glfwWindow.WindowState = (OpenTK.Windowing.Common.WindowState)WindowState.Minimized;

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
            if (args.IsFocused) GLFWWindow.UpdateFrequency = MaxFPS;
            else GLFWWindow.UpdateFrequency = MaxFPSIdle;
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

        private static void UpdateBehaviours()
        {
            // Update scriptable behaviours
            foreach (ScriptableBehaviour behaviour in Scene.current.scriptableBehaviours)
            {
                if (behaviour.enabled) behaviour.Update();
            }

            // Update entities
            foreach (Entity entity in Scene.current.entities)
            {
                if (entity.enabled) entity.Update();
            }

            // Update UI elements and configure transparency
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);

            foreach (UIElement element in Scene.current.uiElements)
            {
                if (element.enabled) element.Update();
            }

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
        }

        private static void ApplyPreLoadSettings()
        {
            // Render Distance
            if (Settings.GetSetting("RenderDistance", out SettingOption renderDistSetting, SettingType.Float))
            {
                Camera.RenderDistance = renderDistSetting.GetValue<float>();
            }

            // Debug Mode
            if (Settings.GetSetting("DebugMode", out SettingOption debugModeSetting, SettingType.Bool))
            {
                Debug.logToConsole = debugModeSetting.GetValue<bool>();
            }

            // Window Title
            if (Settings.GetSetting("WindowTitle", out SettingOption windowTitleSetting, SettingType.String))
            {
                _title = windowTitleSetting.GetValue<string>();
            }

            // Window Size
            if (Settings.GetSetting("WindowSize", out SettingOption windowSizeSetting, SettingType.Vector2))
            {
                _size = (Vector2i)windowSizeSetting.GetValue<Vector2>();
                _halfSize = _size.ToVector2() * 0.5f;
            }

            // VSync
            if (Settings.GetSetting("VSync", out SettingOption vsyncSetting, SettingType.Bool))
            {
                if (vsyncSetting.GetValue<bool>())
                {
                    _vSyncMode = VSyncMode.On;
                }
                else
                {
                    _vSyncMode = VSyncMode.Off;
                }
            }

        }
        private static void ApplyPostLoadSettings()
        {
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
            else Icon = Assets.GetTexture("WindowIcon");

            // Max FPS
            if (Settings.GetSetting("MaxFPS", out SettingOption maxFPSSetting, SettingType.Int))
            {
                MaxFPS = maxFPSSetting.GetValue<int>();
                _glfwWindow.UpdateFrequency = _maxFPS;
            }

            // Max FPS Idle
            if (Settings.GetSetting("MaxFPSIdle", out SettingOption maxFPSIdleSetting, SettingType.Int))
            {
                MaxFPSIdle = maxFPSIdleSetting.GetValue<int>();
            }

            // Cursor
            if (Settings.GetSetting("WindowCursor", out SettingOption cursorTexSetting, SettingType.RefString))
            {
                Vector2i hotspot = Vector2i.Zero;
                Vector2i size = new Vector2i(16, 16);

                if (Settings.GetSetting("WindowCursorHotspot", out SettingOption hotspotSetting, SettingType.Vector2))
                {
                    hotspot = (Vector2i)hotspotSetting.GetValue<Vector2>();
                }
                if (Settings.GetSetting("WindowCursorSize", out SettingOption sizeSetting, SettingType.Vector2))
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
            Debug.LogStatus($"GLFW Version: {glfwMajor}.{glfwMinor}.{glfwRevision}");
            Debug.LogStatus($"OpenGL Version: {GL.GetString(StringName.Version)}");
            Debug.LogStatus($"GLSL Version: {GL.GetString(StringName.ShadingLanguageVersion)}");

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Blend);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            if (Camera.msaaSamples != 0) GL.Enable(EnableCap.Multisample);
        }

        private static NativeWindowSettings GetWindowSettings()
        {
            return new NativeWindowSettings
            {
                Title = _title,
                ClientSize = _size,
                WindowState = (OpenTK.Windowing.Common.WindowState)_windowState,
                StartVisible = false,
                Vsync = _vSyncMode,

                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(4, 5),
                Flags = ContextFlags.Default,

                NumberOfSamples = Camera.msaaSamples,
                DepthBits = 24,
                AutoLoadBindings = true,

                StartFocused = true,
                IsEventDriven = false
            };
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
