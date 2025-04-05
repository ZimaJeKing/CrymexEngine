using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class GradientExample : ScriptableBehaviour
    {
        protected override void Load()
        {
            int textureSize = 64;
            // Create a gradient going from red to blue and add a keypoint for green in the middle
            Gradient gradient = new Gradient(Color4.Red, Color4.Blue);
            gradient.AddKeypoint(0.5f, Color4.Green);

            // Generate a texture directly from the gradient
            Texture texture = Texture.FromGradient(textureSize, textureSize, gradient, GradientDirection.FromLeft);

            // Save the texture if needed
            // texture.Save(Directories.AssetsPath + "GradientImage.png", TextureSaveFormat.PNG);

            // Create an UI element for displaying the texture
            new UIElement(texture, Vector2.Zero, Window.Size, null, "DisplayObject");
        }

        protected override void Update()
        {
        }
    }
}
