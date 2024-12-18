using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public sealed class Button : UIComponent
    {
        public Color4 defaultColor;
        public Color4 hoverColor;
        public Color4 pressedColor;

        public Action<MouseButton> onClick;

        public override void OnMouseExit()
        {
            Renderer.color = defaultColor;
        }

        public override void OnMouseUp()
        {
            Renderer.color = hoverColor;
        }

        public override void OnMouseEnter()
        {
            Renderer.color = hoverColor;
        }

        public override void OnMouseDown(MouseButton mouseButton)
        {
            Renderer.color = pressedColor;
            onClick?.Invoke(mouseButton);
        }
    }
}
