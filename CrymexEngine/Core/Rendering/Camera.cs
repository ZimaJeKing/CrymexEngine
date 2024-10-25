using CrymexEngine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class Camera
    {
        public static Vector2 position;
        public static Color4 clearColor
        {
            set
            {
                GL.ClearColor(value);
            }
        }
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
        public static float renderDistanceSquared {  get; private set; }
        private static float _renderDistance;
    }
}
