using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class BasicEntityInteraction : EntityComponent, IMouseHover, IMouseClick
    {
        public Color4 normalColor = Color4.White;
        public Color4 hoverColor = Color4.LightGray;
        public Color4 pressedColor = Color4.DarkGray;

        public Action<MouseButton> onClick;
        public Action onHoverEnter;
        public Action onHoverExit;

        public void OnMouseDown(MouseButton mouseButton)
        {
            onClick?.Invoke(mouseButton);
            Renderer.color = pressedColor;
        }

        public void OnMouseEnter()
        {
            onHoverEnter?.Invoke();
            Renderer.color = hoverColor;
        }

        public void OnMouseExit()
        {
            onHoverExit?.Invoke();
            Renderer.color = normalColor;
        }

        public void OnMouseHold(MouseButton mouseButton, float time)
        {
        }

        public void OnMouseStay(float time)
        {
        }

        public void OnMouseUp()
        {
            if (Renderer == null) return;

            Renderer.color = hoverColor;
        }

        public override void PreRender()
        {
        }

        protected override void Load()
        {
        }

        protected override void Update()
        {
        }
    }
}
