using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Diagnostics;
namespace CrymexEngine
{
    public sealed class Entity
    {
        public string name;
        public Vector2 position
        {
            get
            {
                return _position;
            }
            set
            {
                if (Parent != null)
                {
                    _localPosition = value - Parent.position;
                }
                _position = value;
                for (int i = 0; i < children.Count; i++)
                {
                    RecalcChildTransform(children[i]);
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
                if (Parent != null)
                {
                    _localRotation = value - Parent.rotation;
                }
                _rotation = value;
                rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation));
                for (int i = 0; i < children.Count; i++)
                {
                    RecalcChildTransform(children[i]);
                }
            }
        }
        public Vector2 scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;

                scaleMatrix = Matrix4.CreateScale(scale.X / (Window.Size.X * 0.5f), scale.Y / (Window.Size.Y * 0.5f), 1);
            }
        }
        public Vector2 localPosition
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
                    position = value;
                    return;
                }
                Parent.RecalcChildTransform(this);
            }
        }
        public float localRotation
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
                    rotation = value; 
                    return; 
                }
                Parent.RecalcChildTransform(this);
                rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation));
            }
        }

        private Vector2 _position;
        private Vector2 _localPosition;
        private float _localRotation;
        private float _rotation;
        private Vector2 _scale;

        public Matrix4 scaleMatrix { get; private set; }
        public Matrix4 rotationMatrix { get; private set; }

        public Entity? Parent
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

        public Renderer renderer { get; private set; }
        public List<EntityComponent> components = new List<EntityComponent>();

        private List<Entity> children = new List<Entity>();

        public static Entity? GetEntity(string name)
        {
            for (int e = 0; e < Scene.Current.entities.Count; e++)
            {
                if (Scene.Current.entities[e].name == name)
                {
                    return Scene.Current.entities[e];
                }
            }
            return null;
        }

        public static Entity? GetEntity(string name, Scene scene)
        {
            for (int e = 0; e < scene.entities.Count; e++)
            {
                if (scene.entities[e].name == name)
                {
                    return scene.entities[e];
                }
            }
            return null;
        }

        public void Update()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].enabled) components[i].Update();
            }
        }

        /// <summary>
        /// Binds an Entity as a child of this object
        /// </summary>
        public void Bind(Entity entity)
        {
            entity.UnbindSelf();
            children.Add(entity);
            entity._parent = this;
            RecalcChildTransform(entity);
        }

        /// <summary>
        /// Unbinds a child at index
        /// </summary>
        public void Unbind(int index)
        {
            if (index < 0 || index >= children.Count) return;
            children[index]._parent = null;
            children[index]._localPosition = Vector2.Zero; 
            children[index]._localRotation = 0;
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
        /// Unbinds itself from the Parent
        /// </summary>
        public void UnbindSelf()
        {
            _parent?.Unbind(this);
        }

        public void PreRender()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].enabled) components[i].PreRender();
            }
        }

        public Entity(Texture texture, Vector2 position, Vector2 scale, string name = "")
        {
            this.position = position;
            this.scale = scale;
            rotation = 0;

            Scene.Current.entities.Add(this);

            renderer = AddComponent<Renderer>();
            renderer.texture = texture;
            this.name = name;
        }
        public Entity(Texture texture, Vector2 position, Vector2 scale, Scene scene, string name = "")
        {
            this.position = position;
            this.scale = scale;
            rotation = 0;

            scene.entities.Add(this);

            renderer = AddComponent<Renderer>();
            renderer.texture = texture;
            this.name = name;
        }

        public T AddComponent<T>() where T : EntityComponent
        {
            object? _instance = Activator.CreateInstance(typeof(T));
            if (_instance == null) return null;

            T instance = (T)_instance;
            instance.Entity = this;
            instance.Load();

            components.Add(instance);

            return instance;
        }

        public bool RemoveComponent<T>() where T : EntityComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    components[i].Entity = null;
                    components[i].enabled = false;
                    components.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public T GetComponent<T>() where T : EntityComponent
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
            scaleMatrix = Matrix4.CreateScale(scale.X / (Window.Size.X * 0.5f), scale.Y / (Window.Size.Y * 0.5f), 1);
            rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation));
        }

        private void RecalcChildTransform(Entity child)
        {
            float dist = child._localPosition.Length;
            float angle = MathF.Atan(child._localPosition.Y / child._localPosition.X);
            angle -= MathHelper.DegreesToRadians(_rotation);
            child._position = _position + new Vector2(dist * MathF.Cos(angle), dist * MathF.Sin(angle));
            child._rotation = _rotation + child._localRotation;
            child.RecalcMatrices();
            for (int i = 0; i < child.children.Count; i++)
            {
                child.RecalcChildTransform(child.children[i]);
            }
        }
    }
}
