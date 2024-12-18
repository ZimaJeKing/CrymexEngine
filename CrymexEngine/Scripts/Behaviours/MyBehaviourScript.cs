using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripting
{
    public class MyBehaviourScript : ScriptableBehaviour
    {
        UIElement beerButton;

        public override void Load()
        {
            beerButton = new UIElement(Assets.GetTexture("Beer"), Vector2.Zero, new Vector2(350));
        }

        public override void Update()
        {
            
        }
    }

    public class ClickableComponent : EntityComponent
    {
        public override void OnMouseEnter()
        {
            Renderer.color = Color4.LightGray;
        }
        public override void OnMouseExit()
        {
            Renderer.color = Color4.White;
        }

        public override void OnMouseDown(MouseButton mouseButton)
        {
            Renderer.color = Color4.Gray;
        }

        public override void OnMouseHold(MouseButton mouseButton, float time)
        {
            if (mouseButton == MouseButton.Right)
            {
                float val = (MathF.Sin(time * 3) + 1) * 0.5f;
                Renderer.color = new Color4(val, 0, val, 1f);
            }
        }
        public override void OnMouseUp()
        {
            Renderer.color = Color4.LightGray;
        }
    }

    public class UIClickableComponent : UIComponent
    {
        public override void OnMouseEnter()
        {
            Renderer.color = Color4.LightGray;
        }
        public override void OnMouseExit()
        {
            Renderer.color = Color4.White;
        }

        public override void OnMouseDown(MouseButton mouseButton)
        {
            Renderer.color = Color4.Gray;
        }

        public override void OnMouseHold(MouseButton mouseButton, float time)
        {
            if (mouseButton == MouseButton.Right)
            {
                float val = (MathF.Sin(time * 3) + 1) * 0.5f;
                Renderer.color = new Color4(1, 1, 1, val);
            }
        }
        public override void OnMouseUp()
        {
            Renderer.color = Color4.LightGray;
        }
    }
}