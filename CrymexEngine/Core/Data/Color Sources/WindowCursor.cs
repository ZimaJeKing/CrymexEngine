using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class WindowCursor
    {
        public readonly Texture texture;
        public readonly Vector2i size;
        public readonly Vector2i hotspot;

        public WindowCursor(Texture texture, Vector2i size, Vector2i hotspot)
        {
            this.texture = texture;
            this.size = size;

            hotspot.X = Math.Clamp(hotspot.X, 0, size.X - 1);
            hotspot.Y = Math.Clamp(hotspot.Y, 0, size.Y - 1);

            this.hotspot = hotspot;
        }
    }
}
