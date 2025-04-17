using OpenTK.Mathematics;

namespace CrymexEngine.GameObjects
{
    public partial class GameObject
    {
        public string name;
        public bool enabled = true;
        public bool interactible = true;
        public bool cursorAlphaTest = true;

        private bool _isBuilt = false;
        private Transform _transform;
        private TagCollection _tagCollection = new();
        protected bool _handlesClickEvents = false;

        protected List<Component> components = new();

        public bool IsBuilt => _isBuilt;
        public bool HandlesClickEvents => _handlesClickEvents;
        public Transform Transform => _transform;

        public GameObject(Vector2 position, Vector2 scale, Transform? parent)
        {
            _transform = new Transform(this, position, scale, parent);

            _isBuilt = true;
        }

        public bool AddTag(string tag) => _tagCollection.AddTag(tag);
        public bool HasTag(string tag) => _tagCollection.HasTag(tag);
        public bool RemoveTag(string tag) => _tagCollection.RemoveTag(tag);

        private void OnMouseEnter()
        {
            foreach (Component component in components)
            {
                if (component is IMouseHover mouseHover)
                {
                    mouseHover.OnMouseEnter();
                }
            }
        }
        private void OnMouseStay(float time)
        {
            foreach (Component component in components)
            {
                if (component is IMouseHover mouseHover)
                {
                    mouseHover.OnMouseStay(time);
                }
            }
        }
        private void OnMouseExit()
        {
            foreach (Component component in components)
            {
                if (component is IMouseHover mouseHover)
                {
                    mouseHover.OnMouseExit();
                }
            }
        }
        private void OnMouseDown(MouseButton mouseButton)
        {
            foreach (Component component in components)
            {
                if (component is IMouseClick mouseClick)
                {
                    mouseClick.OnMouseDown(mouseButton);
                }
            }
        }
        private void OnMouseHold(MouseButton mouseButton, float time)
        {
            foreach (Component component in components)
            {
                if (component is IMouseClick mouseClick)
                {
                    mouseClick.OnMouseHold(mouseButton, time);
                }
            }
        }
        private void OnMouseUp()
        {
            foreach (Component component in components)
            {
                if (component is IMouseClick mouseClick)
                {
                    mouseClick.OnMouseUp();
                }
            }
        }
        private void OnQuit()
        {
            foreach (Component component in components)
            {
                Behaviour.QuitBehaviour(component);
            }
        }

        internal static void GameObjectUpdate(GameObject gameObject) => gameObject.Update();
        internal static void GameObjectPreRender(GameObject gameObject) => gameObject.PreRender();
        internal static void GameObjectCursorEnter(GameObject gameObject) => gameObject.OnMouseEnter();
        internal static void GameObjectCursorStay(GameObject gameObject, float time) => gameObject.OnMouseStay(time);
        internal static void GameObjectCursorExit(GameObject gameObject) => gameObject.OnMouseExit();
        internal static void GameObjectCursorDown(GameObject gameObject, MouseButton button) => gameObject.OnMouseDown(button);
        internal static void GameObjectCursorHold(GameObject gameObject, MouseButton button, float holdTime) => gameObject.OnMouseHold(button, holdTime);
        internal static void GameObjectCursorUp(GameObject gameObject) => gameObject.OnMouseUp();
        internal static void GameObjectQuit(GameObject gameObject) => gameObject.OnQuit();

        protected virtual void Update() { }
        protected virtual void PreRender() { }
        public virtual void Delete() { }
    }
}
