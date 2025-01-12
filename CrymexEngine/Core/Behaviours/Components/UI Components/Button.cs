using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public sealed class Button : UIComponent, IMouseClick, IMouseHover
    {
        public Color4 defaultColor = Color4.White;
        public Color4 hoverColor = Color4.WhiteSmoke;
        public Color4 pressedColor = Color4.LightGray;

        public bool colorTransitions = true;

        public Action<MouseButton> onClick;
        public Action onHoverEnter;
        public Action onHoverExit;

        public void OnMouseExit()
        {
            onHoverExit?.Invoke();

            if (!colorTransitions) return;
            Renderer.color = defaultColor;
        }

        public void OnMouseUp()
        {
            if (!colorTransitions) return;
            Renderer.color = hoverColor;
        }

        public void OnMouseEnter()
        {
            onHoverEnter?.Invoke();

            if (!colorTransitions) return;
            Renderer.color = hoverColor;
        }

        public void OnMouseDown(MouseButton mouseButton)
        {
            onClick?.Invoke(mouseButton);
            if (!colorTransitions) return;
            Renderer.color = pressedColor;
        }

        public override void PreRender() { }

        protected override void Load() { }

        protected override void Update() { }

        public void OnMouseHold(MouseButton mouseButton, float time) { }

        public void OnMouseStay(float time) { }
    }
}
