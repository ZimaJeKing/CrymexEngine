using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public sealed class Button : UIComponent
    {
        public Color4 defaultColor = Color4.White;
        public Color4 hoverColor = Color4.WhiteSmoke;
        public Color4 pressedColor = Color4.LightGray;

        public bool colorTransitions = true;

        public Action<MouseButton> onClick;
        public Action onHoverEnter;
        public Action onHoverExit;

        public override void OnMouseExit()
        {
            onHoverExit?.Invoke();

            if (!colorTransitions) return;
            Renderer.color = defaultColor;
        }

        public override void OnMouseUp()
        {
            if (!colorTransitions) return;
            Renderer.color = hoverColor;
        }

        public override void OnMouseEnter()
        {
            onHoverEnter?.Invoke();

            if (!colorTransitions) return;
            Renderer.color = hoverColor;
        }

        public override void OnMouseDown(MouseButton mouseButton)
        {
            onClick?.Invoke(mouseButton);

            if (!colorTransitions) return;
            Renderer.color = pressedColor;
        }
    }
}
