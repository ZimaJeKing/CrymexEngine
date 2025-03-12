using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class SettingsExample : ScriptableBehaviour
    {
        Settings? settings;
        TextObject textDisplay;

        protected override void Load()
        {
            textDisplay = new TextObject(Vector2.Zero, Window.Size, "No settings loaded", Assets.DefaultFontFamily, 0);

            settings = Assets.GetSettings("MyConfig");
            if (settings != null)
            {
                textDisplay.Text = settings.AsText;
            }
        }

        protected override void Update()
        {
        }
    }
}
