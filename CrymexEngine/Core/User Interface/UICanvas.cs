using CrymexEngine.Scenes;
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

        internal void Update()
        {
            HandleMouseInput();
        }

        public void SortElements()
        {
            Scene.Current.uiElements.Sort((a, b) => -b.Renderer.Depth.CompareTo(a.Renderer.Depth));
        }

        private static void HandleMouseInput()
        {
            // Find overlaping entities and filter entities by EntityRenderer.Depth
            GameObject? hover = null;
            float highestDepth = float.MinValue;
            foreach (Entity entity in Scene.Current.entities)
            {
                if (entity.enabled && entity.Renderer != null && entity.Renderer.enabled && entity.interactible && entity.HandlesClickEvents && Input.CursorOverlap(entity, entity.cursorAlphaTest))
                {
                    if (entity.Renderer.Depth >= highestDepth)
                    {
                        hover = entity;
                        highestDepth = entity.Renderer.Depth;
                    }
                }
            }

            highestDepth = float.MinValue;
            foreach (UIElement element in Scene.Current.uiElements)
            {
                if (element.enabled && element.interactible && element.HandlesClickEvents && Input.CursorOverlap(element, element.cursorAlphaTest))
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
                if (_hover != null) GameObject.GameObjectCursorExit(_hover);
                _hover = null;
                return;
            }

            if (hover != _hover)
            {
                if (_hover != null) GameObject.GameObjectCursorExit(_hover);
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
                GameObject.GameObjectCursorEnter(gameObject);
                if (_hold == _hover)
                {
                    _hold = gameObject;
                }
                _hover = gameObject;
                _hoverTime = Time.GameTime;
            }
            else
            {
                GameObject.GameObjectCursorStay(gameObject, Time.GameTime - _hoverTime);
            }

            // No input
            if (input == null)
            {
                if (_hold != null) GameObject.GameObjectCursorUp(gameObject);
                _hold = null;
                return;
            }

            // Click events
            if (_hold == null)
            {
                GameObject.GameObjectCursorDown(gameObject, input.Value);
                _hold = gameObject;
                _holdTime = Time.GameTime;
            }
            else
            {
                GameObject.GameObjectCursorHold(gameObject, input.Value, Time.GameTime - _holdTime);
            }
        }
    }
}
