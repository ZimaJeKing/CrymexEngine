using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public static class UICanvas
    {
        private static Entity? _holdEntity;
        private static Entity? _hoverEntity;

        private static float _holdTime, _hoverTime;

        public static void Update()
        {
            HandleMouseInput();
        }

        private static void HandleMouseInput()
        {
            foreach (UIElement element in Scene.current.uiElements)
            {

            }

            // Find overlaping entities and flter entities renderer.Depth
            Entity? hover = null;
            float highestDepth = float.MinValue;
            foreach (Entity entity in Scene.current.entities)
            {
                if (entity.renderer != null && Input.CursorOverlap(entity))
                {
                    if (entity.renderer.Depth >= highestDepth)
                    {
                        hover = entity;
                        highestDepth = entity.renderer.Depth;
                    }
                }
            }

            if (hover == null)
            {
                _hoverEntity?.OnCursorExit();
                _hoverEntity = null;
                return;
            }
            else if (hover != _hoverEntity)
            {
                _hoverEntity?.OnCursorExit();
            }

            HandleCursorLogic(hover);
        }

        private static void HandleCursorLogic(Entity entity)
        {
            MouseButton? input = null;
            if (Input.Mouse(MouseButton.Left)) input = MouseButton.Left;
            if (Input.Mouse(MouseButton.Right)) input = MouseButton.Right;
            if (Input.Mouse(MouseButton.Middle)) input = MouseButton.Middle;

            // Hover events
            if (_hoverEntity == null || _hoverEntity != entity)
            {
                entity.OnCursorEnter();
                if (_holdEntity == _hoverEntity)
                {
                    _holdEntity = entity;
                }
                _hoverEntity = entity;
                _hoverTime = Time.GameTime;
            }
            else
            {
                entity.OnCursorStay(Time.GameTime - _hoverTime);
            }

            // No input
            if (input == null)
            {
                _holdEntity?.OnCursorUp();
                _holdEntity = null;
                return;
            }

            // Click events
            if (_holdEntity == null)
            {
                entity.OnCursorDown(input.Value);
                _holdEntity = entity;
                _holdTime = Time.GameTime;
            }
            else
            {
                entity.OnCursorHold(input.Value, Time.GameTime - _holdTime);
            }
        }
    }
}
