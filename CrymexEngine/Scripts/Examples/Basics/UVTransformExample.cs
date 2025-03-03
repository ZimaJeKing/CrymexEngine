using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class UVTransformExample : ScriptableBehaviour
    {
        InputField uvTilingField;
        InputField uvOffsetField;

        UIElement display;
        protected override void Load()
        {
            // This example shows how to use repeat tiling
            // Made for a 720x720 window

            // Make a display element
            Texture texture = Assets.GetTexture("Car");
            display = new UIElement(texture, Vector2.Zero, new Vector2(500), null, "TilingDisplay");

            UIElement uvTilingElement = new UIElement(Texture.White, new Vector2(-150, 300), new Vector2(256, 64), null, "TilingField");
            UIElement uvOffsetElement = new UIElement(Texture.White, new Vector2(150, 300), new Vector2(256, 64), null, "OffsetField");
            uvTilingField = uvTilingElement.AddComponent<InputField>();
            uvOffsetField = uvOffsetElement.AddComponent<InputField>();

            uvTilingField.CharacterLimit = 5;
            uvTilingField.onSubmit = SubmitTiling;
            uvTilingField.PreviewText = "Tiling";

            uvOffsetField.CharacterLimit = 5;
            uvOffsetField.onSubmit = SubmitOffset;
            uvOffsetField.PreviewText = "Offset";
        }

        protected override void Update()
        {
            
        }

        private void SubmitTiling(string value)
        {
            if (!float.TryParse(value, out float valueNum) && valueNum > 0 && valueNum < 10000)
            {
                Debug.Log("Something went wrong with the tiling input");
                return;
            }

            display.Renderer.UVTiling = new Vector2(valueNum);
        }
        private void SubmitOffset(string value)
        {
            if (!float.TryParse(value, out float valueNum) && valueNum > 0 && valueNum < 10000)
            {
                Debug.Log("Something went wrong with the offset input");
                return;
            }

            display.Renderer.UVOffset = new Vector2(valueNum);
        }
    }
}
