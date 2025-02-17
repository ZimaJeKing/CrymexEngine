using CrymexEngine.Data;
using CrymexEngine.Rendering;
using CrymexEngine.UI;
using CrymexEngine.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts
{
    public class MyBehaviourScript : ScriptableBehaviour
    {
        UIElement display;
        protected override void Load()
        {
            display = new UIElement(Assets.GetTexture("Car"), Vector2.Zero, new Vector2(500));
        }

        protected override void Update()
        {
            display.Rotation = Time.GameTime * 10;
        }
    }
}