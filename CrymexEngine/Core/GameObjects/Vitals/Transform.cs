using OpenTK.Mathematics;

namespace CrymexEngine.GameObjects
{
    public class Transform
    {
        private Vector2 _halfScale;
        private Vector2 _localPosition = Vector2.Zero;
        private float _localRotation = 0;
        private Vector2 _position = Vector2.Zero;
        private float _rotation = 0;
        private Vector2 _scale = Vector2.Zero;
        private Matrix4 _transformationMatrix;
        private Transform? _parent;

        protected List<Transform> children = new();

        public Transform(GameObject gameObject, Vector2 position, Vector2 scale, Transform? parent)
        {
            if (gameObject.IsBuilt) throw new Exception("Game object vitals cannot be instantiated on built game objects");

            _position = position;
            _scale = scale;
            _halfScale = scale * 0.5f;

            if (parent != null)
            {
                _parent = parent;
                parent.children.Add(this);
                _position = Vector2.Zero;
                LocalPosition = position;
            }

            RecalcTransformMatrix();
        }

        public Vector2 HalfScale => _halfScale;
        public Matrix4 TransformationMatrix => _transformationMatrix;

        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;

                if (_parent != null)
                {
                    _localPosition = value - _parent._position;
                }

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
                _rotation = value;

                if (_parent != null)
                {
                    _localRotation = value - _parent._rotation;
                }

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
                else _position = value - Parent.Position;
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
                else _rotation = value - Parent.Rotation;
                Parent.RecalcChildTransform(this);
                RecalcTransformMatrix();
            }
        }

        public Transform? Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (value == null || value == this) { UnbindSelf(); return; }
                value.BindChild(this);
                _localPosition = _position - value._position;
                _localRotation = _rotation - value._rotation;
            }
        }

        protected void RecalcTransformMatrix()
        {
            _transformationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_rotation)) * Matrix4.CreateScale(_scale.X / Window.HalfSize.X, _scale.Y / Window.HalfSize.Y, 1);
        }

        private void RecalcChildTransform(Transform child)
        {
            float dist = child._localPosition.Length;

            float angle = MathF.Atan2(child._localPosition.Y, child._localPosition.X);

            angle -= MathHelper.DegreesToRadians(_rotation);

            child._position = _position + new Vector2(dist * MathF.Cos(angle), dist * MathF.Sin(angle));
            child._rotation = _rotation + child._localRotation;
            child.RecalcTransformMatrix();
        }

        private void BindChild(Transform gameObject)
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
