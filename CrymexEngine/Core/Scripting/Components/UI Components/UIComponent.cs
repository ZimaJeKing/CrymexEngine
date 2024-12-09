using System;

namespace CrymexEngine.UI
{
    public class UIComponent : Component
    {
        public UIRenderer Renderer
        {
            get
            {
                return (UIRenderer)_renderer;
            }
        }

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

                _renderer = value.Renderer;
                _uiElement = value;
            }
        }

        private UIElement? _uiElement;
    }
}
