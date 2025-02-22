using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class GradientExample : ScriptableBehaviour
    {
        protected override void Load()
        {
            int textureSize = 64;
            // Create a gradient going from red, to green, to blue
            Gradient gradient = new Gradient(Color4.Red, Color4.Blue);
            gradient.AddKeypoint(0.5f, Color4.Green);

            // Sample the gradient by the x coordinate
            Texture texture = new Texture(textureSize, textureSize);
            for (int x = 0; x < textureSize; x++)
            {
                Color4 color = gradient.GetValue((float)x / textureSize);
                for (int y = 0; y < textureSize; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            //texture.Save(Directories.AssetsPath + "GradientImage.png", TextureSaveFormat.PNG);

            // Create a display texture
            UIElement display = new UIElement(texture, Vector2.Zero, Window.Size, null, "DisplayObject");
        }

        protected override void Update()
        {
        }
    }
}
