using System;

namespace CrymexEngine.UI
{
    public class UIComponent
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
        public bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_uiElement == null) return;

                _enabled = value;
            }
        }

        private bool _enabled = true;
        private UIElement? _uiElement;

        public virtual void Load() { }
        public virtual void Refresh() { }
        public virtual void Update() { }
        public virtual void Click(Mouse mouseButton) { }
        public virtual void CursorEnter() { }
        public virtual void CursorExit() { }
    }
}
