using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public class UICanvas
    {
        /// <summary>
        /// An internal instance
        /// </summary>
        public static UICanvas Instance
        {
            get
            {
                return _instance;
            }
        }

        private static GameObject? _hold;
        private static GameObject? _hover;

        private static float _holdTime, _hoverTime;

        private static UICanvas _instance = new UICanvas();

        public void Update()
        {
            HandleMouseInput();
        }

        public void SortElements()
        {
            Scene.current.uiElements.Sort((a, b) => -b.Renderer.Depth.CompareTo(a.Renderer.Depth));
        }

        private static void HandleMouseInput()
        {
            // Find overlaping entities and flter entities EntityRenderer.Depth
            GameObject? hover = null;
            float highestDepth = float.MinValue;
            foreach (Entity entity in Scene.current.entities)
            {
                if (entity.enabled && entity.Renderer != null && entity.Renderer.enabled && Input.CursorOverlap(entity))
                {
                    if (entity.Renderer.Depth >= highestDepth)
                    {
                        hover = entity;
                        highestDepth = entity.Renderer.Depth;
                    }
                }
            }

            highestDepth = float.MinValue;
            foreach (UIElement element in Scene.current.uiElements)
            {
                if (element.enabled && Input.CursorOverlap(element))
                {
                    if (element.Renderer.Depth >= highestDepth)
                    {
                        hover = element;
                        highestDepth = element.Renderer.Depth;
                    }
                }
            }


            if (hover == null)
            {
                _hover?.OnCursorExit();
                _hover = null;
                return;
            }
            else if (hover != _hover)
            {
                _hover?.OnCursorExit();
            }

            HandleCursorLogic(hover);
        }

        private static void HandleCursorLogic(GameObject gameObject)
        {
            MouseButton? input = null;
            if (Input.Mouse(MouseButton.Left)) input = MouseButton.Left;
            if (Input.Mouse(MouseButton.Right)) input = MouseButton.Right;
            if (Input.Mouse(MouseButton.Middle)) input = MouseButton.Middle;

            // Hover events
            if (_hover == null || _hover != gameObject)
            {
                gameObject.OnCursorEnter();
                if (_hold == _hover)
                {
                    _hold = gameObject;
                }
                _hover = gameObject;
                _hoverTime = Time.GameTime;
            }
            else
            {
                gameObject.OnCursorStay(Time.GameTime - _hoverTime);
            }

            // No input
            if (input == null)
            {
                _hold?.OnCursorUp();
                _hold = null;
                return;
            }

            // Click events
            if (_hold == null)
            {
                gameObject.OnCursorDown(input.Value);
                _hold = gameObject;
                _holdTime = Time.GameTime;
            }
            else
            {
                gameObject.OnCursorHold(input.Value, Time.GameTime - _holdTime);
            }
        }
    }
}
