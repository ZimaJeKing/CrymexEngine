using System;

namespace CrymexEngine.UI
{
    public class UIComponent : Behaviour
    {
        public UIRenderer renderer;
        public UIElement UIElement
        {
            get
            {
                return _uiElement;
            }
            set
            {
                if (value == null)
                {
                    enabled = false;
                    _uiElement = null;
                    return;
                }

                _uiElement = value;
            }
        }

        private UIElement? _uiElement;

        public virtual void Click(MouseButton mouseButton) { }
        public virtual void CursorEnter() { }
        public virtual void CursorExit() { }
    }
}
