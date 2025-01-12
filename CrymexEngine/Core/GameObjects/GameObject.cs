using OpenTK.Mathematics;

namespace CrymexEngine
{
    public partial class GameObject
    {
        public bool enabled = true;
        public bool interactible = true;
        public bool cursorAlphaTest = true;

        public string name;
        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;

                for (int i = 0; i < children.Count; i++)
                {
                    RecalcChildTransform(children[i]);
                }
            }
        }
        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                if (Parent != null) return;
                _rotation = value;

                RecalcTransformMatrix();

                for (int i = 0; i < children.Count; i++)
                {
                    RecalcChildTransform(children[i]);
                }
            }
        }
        public Vector2 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                _halfScale = value * 0.5f;

                RecalcTransformMatrix();
            }
        }
        public Vector2 LocalPosition
        {
            get
            {
                return _localPosition;
            }
            set
            {
                _localPosition = value;
                if (Parent == null)
                {
                    Position = value;
                    return;
                }
                Parent.RecalcChildTransform(this);
            }
        }
        public float LocalRotation
        {
            get
            {
                if (Parent == null) return _rotation;
                return _localRotation;
            }
            set
            {
                _localRotation = value;
                if (Parent == null)
                {
                    Rotation = value;
                    return;
                }
                Parent.RecalcChildTransform(this);
                RecalcTransformMatrix();
            }
        }
        public GameObject? Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (value == null || value == this) { UnbindSelf(); return; }
                value.BindChild(this);
            }
        }
        public bool HandlesClickEvents => _handlesClickEvents;

        public Vector2 HalfScale
        {
            get
            {
                return _halfScale;
            }
        }
        public Matrix4 TransformationMatrix { get; private set; }

        private Vector2 _halfScale;
        private Vector2 _localPosition = Vector2.Zero;
        private float _localRotation;
        private Vector2 _position = Vector2.Zero;
        private float _rotation;
        private Vector2 _scale = Vector2.Zero;
        private GameObject? _parent;
        protected bool _handlesClickEvents;

        protected List<GameObject> children = new List<GameObject>();
        protected List<Component> components = new List<Component>();

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

        public static void GameObjectUpdate(GameObject gameObject) => gameObject.Update();
        public static void GameObjectPreRender(GameObject gameObject) => gameObject.PreRender();
        public static void GameObjectCursorEnter(GameObject gameObject) => gameObject.OnMouseEnter();
        public static void GameObjectCursorStay(GameObject gameObject, float time) => gameObject.OnMouseStay(time);
        public static void GameObjectCursorExit(GameObject gameObject) => gameObject.OnMouseExit();
        public static void GameObjectCursorDown(GameObject gameObject, MouseButton button) => gameObject.OnMouseDown(button);
        public static void GameObjectCursorHold(GameObject gameObject, MouseButton button, float holdTime) => gameObject.OnMouseHold(button, holdTime);
        public static void GameObjectCursorUp(GameObject gameObject) => gameObject.OnMouseUp();

        protected void RecalcTransformMatrix()
        {
            TransformationMatrix = Matrix4.CreateScale(_scale.X / Window.HalfSize.X, _scale.Y / Window.HalfSize.Y, 1) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation));
        }

        protected virtual void Update() { }
        protected virtual void PreRender() { }
        public virtual void Delete() { }

        private void RecalcChildTransform(GameObject child)
        {
            float dist = child._localPosition.Length;
            float angle = MathF.Atan(child._localPosition.Y / child._localPosition.X);

            // Handles a case, where child._localPosition.X is 0
            if (!float.IsNormal(angle)) angle = 0;

            angle -= MathHelper.DegreesToRadians(_rotation);

            child.Position = Position + new Vector2(dist * MathF.Cos(angle), dist * MathF.Sin(angle));
            child.Rotation = Rotation + child._localRotation;
            child.RecalcTransformMatrix();
        }

        private void BindChild(GameObject gameObject)
        {
            gameObject.UnbindSelf();
            children.Add(gameObject);
            gameObject._parent = this;

            gameObject._localPosition = gameObject.Position - Position;
            gameObject._localRotation = gameObject.Rotation - Rotation;

            RecalcChildTransform(gameObject);
        }

        private void UnbindSelf()
        {
            _parent?.children.Remove(this);
            _parent = null;
        }
    }
}
