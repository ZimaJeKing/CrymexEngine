using System;

namespace CrymexEngine.UI
{
    public abstract class UIComponent : Component
    {
        public UIRenderer Renderer
        {
            get
            {
                return (UIRenderer)_rendererComponent;
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

                _rendererComponent = value.Renderer;
                _uiElement = value;
            }
        }

        private UIElement? _uiElement;
    }
}
