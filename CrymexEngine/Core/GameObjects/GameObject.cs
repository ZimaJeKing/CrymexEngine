using OpenTK.Mathematics;

namespace CrymexEngine
{
    public partial class GameObject
    {
        public bool enabled = true;
        public bool interactible = true;

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

        protected List<GameObject> children = new List<GameObject>();
        protected List<Component> components = new List<Component>();

        public void OnCursorEnter()
        {
            foreach (Component component in components)
            {
                component.OnMouseEnter();
            }
        }
        public void OnCursorStay(float time)
        {
            foreach (Component component in components)
            {
                component.OnMouseStay(time);
            }
        }
        public void OnCursorExit()
        {
            foreach (Component component in components)
            {
                component.OnMouseExit();
            }
        }
        public void OnCursorDown(MouseButton mouseButton)
        {
            foreach (Component component in components)
            {
                component.OnMouseDown(mouseButton);
            }
        }
        public void OnCursorHold(MouseButton mouseButton, float time)
        {
            foreach (Component component in components)
            {
                component.OnMouseHold(mouseButton, time);
            }
        }
        public void OnCursorUp()
        {
            foreach (Component component in components)
            {
                component.OnMouseUp();
            }
        }

        protected void RecalcTransformMatrix()
        {
            TransformationMatrix = Matrix4.CreateScale(_scale.X / Window.HalfSize.X, _scale.Y / Window.HalfSize.Y, 1) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation));
        }

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
