using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class Camera
    {
        public static Camera MainCamera
        {
            get
            {
                return _instance;
            }
        }

        public static Vector2 position;

        /// <summary>
        /// Window background color
        /// </summary>
        public static Color4 ClearColor
        {
            get
            {
                return _clearColor;
            }
            set
            {
                _clearColor = value;
                GL.ClearColor(value);
            }
        }

        /// <summary>
        /// A distance at which objects are no longer rendered
        /// </summary>
        public static float RenderDistance
        {
            get
            {
                return _renderDistance;
            }
            set
            {
                _renderDistance = value;
                renderDistanceSquared = value * value;
            }
        }

        /// <summary>
        /// How many samples to use with MSAA
        /// </summary>
        public static int msaaSamples { get; private set; } = 0;

        /// <summary>
        /// Used for quick calculation of visibility of objects
        /// </summary>
        public static float renderDistanceSquared { get; private set; } = float.MaxValue;

        private static float _renderDistance = float.MaxValue;
        private static Color4 _clearColor;

        private static Camera _instance = new Camera();

        public void Init()
        {
            if (Window.Loaded) return;

            // Clear color
            _clearColor = Color4.White;
            if (Settings.GlobalSettings.GetSetting("ClearColor", out SettingOption clearColorSetting, SettingType.Hex))
            {
                _clearColor = System.Drawing.Color.FromArgb(clearColorSetting.GetValue<int>());
            }
            GL.ClearColor(_clearColor);

            // MSAA settings
            if (Settings.GlobalSettings.GetSetting("MSAASamples", out SettingOption msaaSetting))
            {
                int maxMsaaSamples;
                GL.GetInteger(GetPName.MaxSamples, out maxMsaaSamples);

                Debug.LogLocalInfo("Camera", "Max supported MSAA samples: " + maxMsaaSamples);

                if (msaaSamples <= maxMsaaSamples)
                {
                    if (msaaSamples == 16 || msaaSamples == 8 || msaaSamples == 4 || msaaSamples == 2)
                    {
                        msaaSamples = (int)msaaSetting.value;
                    }
                }
            }
        }

        public static Vector2 ScreenSpaceToWorldSpace(Vector2 point)
        {
            return point + position;
        }
        public static Vector2 WorldSpaceToScreenSpace(Vector2 point)
        {
            return point - position;
        }
    }
}
