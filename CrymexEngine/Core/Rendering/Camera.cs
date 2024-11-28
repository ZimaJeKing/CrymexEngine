using CrymexEngine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class Camera
    {
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
        public static float renderDistance
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
        public static float renderDistanceSquared { get; private set; } = 2048 * 2048;

        private static float _renderDistance = 2048;
        private static Color4 _clearColor;

        public static void Init()
        {
            // Clear color
            _clearColor = Color4.White;
            if (Settings.GetSetting("ClearColor", out SettingOption clearColorSetting))
            {
                _clearColor = System.Drawing.Color.FromArgb(clearColorSetting.GetValue<int>());
            }
            GL.ClearColor(_clearColor);

            // MSAA settings
            if (Settings.GetSetting("MSAASamples", out SettingOption msaaSetting))
            {
                if (msaaSamples == 8 || msaaSamples == 4 || msaaSamples == 2)
                {
                    msaaSamples = (int)msaaSetting.value;
                }
            }
        }
    }
}
