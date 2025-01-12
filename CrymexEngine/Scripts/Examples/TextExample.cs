using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Examples
{
    public class TextExample : ScriptableBehaviour
    {
        TextObject text;
        int counter = 0;

        protected override void Load()
        {
            // Create a text object
            text = new TextObject(new Vector2(0, 0), new Vector2i(200, 50), "Count: 0", Assets.GetFontFamily("Jakarta"), 20, Alignment.MiddleCenter, SixLabors.Fonts.FontStyle.Regular);

            // Create a transparent button to register click events
            UIElement buttonObject = new UIElement(Texture.White, new Vector2(0, 0), new Vector2(200, 50));
            Button buttonComponent = buttonObject.AddComponent<Button>();
            buttonComponent.onClick += Click;
        }

        protected override void Update()
        {
        }

        private void Click(MouseButton button)
        {
            counter++;
            text.Text = "Count: " + counter;
        }
    }
}