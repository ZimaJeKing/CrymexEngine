using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Diagnostics;
namespace CrymexEngine
{
    public sealed class Entity
    {
        public Vector2 position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                RecalcMatrices();
                for (int i = 0; i < children.Count; i++)
                {
                    Entity child = children[i];
                    float dist = Vector2.Distance(child._position, position);
                    float angle = MathF.Atan2(_position.Y - child._position.Y, _position.X - child._position.X);
                    child.position = new Vector2(dist * MathF.Sin(angle), dist * MathF.Cos(angle));
                }
            }
        }
        public float rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                for (int i = 0; i < children.Count; i++)
                {
                    Entity child = children[i];
                    float dist = Vector2.Distance(child._position, _position);
                    float angle = MathF.Atan(_position.Y - child._position.Y / _position.X - child._position.X);
                    Debug.LogL(angle);
                    child.position = new Vector2(dist * MathF.Sin(angle + _rotation), dist * MathF.Cos(angle));
                    child.rotation = child._localRotation;
                }
                RecalcMatrices();
            }
        }
        public Vector2 scale;
        public Vector2 localPosition
        {
            get
            {
                if (parent == null) return _position;
                return _localPosition;
            }
            set
            {
                _localPosition = value;
                if (parent == null)
                {
                    _position = value;
                    return;
                }

                position = parent._position + value;

                RecalcMatrices();
            }
        }
        private float _localRotation;
        public float localRotation
        {
            get
            {
                if (parent == null) return _rotation;
                return _localRotation;
            }
            set
            {
                if (parent == null) { _rotation = value; return; }
                _rotation = parent.rotation + value;
                _localRotation = value;
                RecalcMatrices();
            }
        }

        private Vector2 _position;
        private Vector2 _localPosition;
        private float _rotation;

        public Matrix4 scaleMatrix { get; private set; }
        public Matrix4 rotationMatrix { get; private set; }

        public Entity? parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (value == null) { UnbindSelf(); return; }
                value.Bind(this);
            }
        }
        private Entity? _parent;

        public Renderer renderer;
        public List<Component> components = new List<Component>();

        private List<Entity> children = new List<Entity>();

        public void Update()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].enabled) components[i].Update();
            }
        }

        /// <summary>
        /// Binds an entity as a child of this object
        /// </summary>
        public void Bind(Entity entity)
        {
            entity.UnbindSelf();
            children.Add(entity);
            entity._parent = this;
        }

        /// <summary>
        /// Unbinds a child at index
        /// </summary>
        public void Unbind(int index)
        {
            if (index < 0 || index >= children.Count) return;
            children[index]._parent = null;
            children.RemoveAt(index);
        }
        /// <summary>
        /// Unbinds a child
        /// </summary>
        public void Unbind(Entity entity)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (entity == children[i])
                {
                    children[i]._parent = null;
                    children.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Unbinds itself from the parent
        /// </summary>
        public void UnbindSelf()
        {
            _parent?.Unbind(this);
        }

        public void Render()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].enabled) components[i].Render();
            }
        }

        public Entity(Texture texture, Vector2 position, Vector2 scale)
        {
            this.position = position;
            this.scale = scale;
            rotation = 0;

            Scene.entities.Add(this);

            renderer = AddComponent<Renderer>();
            renderer.texture = texture;
        }

        public T AddComponent<T>() where T : Component
        {
            object? _instance = Activator.CreateInstance(typeof(T));
            if (_instance == null) return null;

            T instance = (T)_instance;
            instance.entity = this;
            instance.renderer = renderer;
            instance.Load();

            components.Add(instance);

            return instance;
        }

        public bool RemoveComponent<T>() where T : Component
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    components[i].entity = null;
                    components.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    return (T)components[i];
                }
            }
            return null;
        }

        public void RecalcMatrices()
        {
            scaleMatrix = Matrix4.CreateScale(scale.X / (Program.window.Size.X * 0.5f), scale.Y / (Program.window.Size.Y * 0.5f), 1);
            rotationMatrix = Matrix4.CreateRotationZ(rotation);
        }
    }
}
