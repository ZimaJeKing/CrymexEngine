using System;

namespace CrymexEngine.UI
{
    public abstract class UIComponent : Component
    {
        public UIRenderer Renderer
        {
            get
            {
                if (rendererComponent != null) return (UIRenderer)rendererComponent;
                return null;
            }
        }

        public UIElement Element
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

                rendererComponent = value.Renderer;
                _uiElement = value;
            }
        }

        private UIElement? _uiElement;
    }
}
