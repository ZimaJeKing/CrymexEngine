using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Examples
{
    public class InputFieldExample : ScriptableBehaviour
    {
        // Made for a 512x512 window
        protected override void Load()
        {
            UIElement inputFieldElement = new UIElement(Texture.White, new Vector2(0, 0), new Vector2(512, 64), null, "InputField", 0);
            InputField inputField = inputFieldElement.AddComponent<InputField>();
            inputField.CharacterLimit = 30;
            inputField.DisplayText.BestFit = false;
            inputField.DisplayText.FontSize = 32;
            inputField.PreviewText = "Type something here...";
        }

        protected override void Update()
        {
        }
    }
}
