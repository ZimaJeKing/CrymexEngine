using CrymexEngine.Rendering;
using CrymexEngine.UI;
using CrymexEngine.Utils;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts
{
    public class MyBehaviourScript : ScriptableBehaviour
    {
        protected override void Load()
        {
            UIElement car = new UIElement(Assets.GetTexture("Car"), Vector2.Zero, new Vector2(256));
        }

        protected override void Update()
        {
            
        }
    }
}