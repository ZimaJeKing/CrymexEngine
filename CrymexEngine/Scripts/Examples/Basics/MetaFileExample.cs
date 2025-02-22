using CrymexEngine.Data;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class MetaFileExample : ScriptableBehaviour
    {
        protected override void Load()
        {
            // This example loads a texture along with its asset data and reads from its metafile

            string textureName = "Car";
            string customPropertyName = "MyCustomProperty";

            // Load the texture asset
            TextureAsset? textureAsset = Assets.GetTextureAsset(textureName);
            if (textureAsset == null)
            {
                Debug.Log($"Texture '{textureName}' not found");
                return;
            }

            // Create an entity for displaying the texture
            new Entity(textureAsset.texture, Vector2.Zero, Window.Size, null, "TextureDisplay");

            // Read the custom metafile property (string property in this case)
            MetaFile? meta = textureAsset.Meta;
            if (meta != null)
            {
                // Read the string property
                string? customValue = meta.GetStringProperty(customPropertyName);

                if (customValue == null)
                {
                    Debug.Log($"Custom property not found. You have to manualy set its value at '{textureAsset.path}'");
                    return;
                }

                // Log the custom value
                Debug.Log($"Custom property value: {customValue}");
            }
            else
            {
                Debug.Log("MetaFile not found. Try enabling metafiles in global settings (UseMetaFiles: True)");
            }
        }

        protected override void Update()
        {
        }
    }
}
