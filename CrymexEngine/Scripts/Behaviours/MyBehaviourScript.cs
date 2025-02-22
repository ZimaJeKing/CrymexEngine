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
        UIElement display = new UIElement(Assets.GetTextureBroad("Car"), Vector2.Zero, new Vector2(500));
        protected override void Load()
        {
            
        }

        protected override void Update()
        {
            display.Rotation = Time.GameTime * 10;
        }
    }
}