using OpenTK.Mathematics;
namespace CrymexEngine
{
    public sealed class Entity : GameObject
    {
        public Renderer? renderer
        {
            get
            {
                return _renderer;
            }
            private set
            {
                _renderer = value;
            }
        }

        private List<EntityComponent> components = new List<EntityComponent>();

        private Renderer? _renderer;

        public static Entity? GetEntity(string name)
        {
            for (int e = 0; e < Scene.current.entities.Count; e++)
            {
                if (Scene.current.entities[e].name == name)
                {
                    return Scene.current.entities[e];
                }
            }
            return null;
        }
        public static Entity? GetEntity(string name, Scene scene)
        {
            foreach (Entity entity in scene.entities)
            {
                if (entity.name == name)
                {
                    return entity;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            foreach (EntityComponent component in components)
            {
                if (component.enabled)
                {
                    component.Update();
                }
            }
        }

        public void PreRender()
        {
            foreach (EntityComponent component in components)
            {
                if (component.enabled)
                {
                    component.PreRender();
                }
            }
        }

        public Entity(Texture texture, Vector2 position, Vector2 scale, Entity? parent = null, string name = "")
        {
            Parent = parent;
            Position = position;
            Scale = scale;
            Rotation = 0;

            Scene.current.entities.Add(this);

            renderer = AddComponent<Renderer>();
            renderer.texture = texture;
            this.name = name;
        }

        /// <summary>
        /// Creates an Entity in the specified scene
        /// </summary>
        public Entity(Texture texture, Vector2 position, Vector2 scale, Scene scene, Entity? parent = null, string name = "")
        {
            Parent = parent;
            Position = position;
            Scale = scale;
            Rotation = 0;

            scene.entities.Add(this);

            renderer = AddComponent<Renderer>();
            renderer.texture = texture;
            this.name = name;
        }

        /// <summary>
        /// Creates an Entity with no Renderer attached
        /// </summary>
        public Entity(Vector2 position, Vector2 scale, Entity? parent = null, string name = "")
        {
            Parent = parent;
            Position = position;
            Scale = scale;
            Rotation = 0;

            Scene.current.entities.Add(this);
        }

        public T AddComponent<T>() where T : EntityComponent
        {
            object? _instance = Activator.CreateInstance(typeof(T));
            if (_instance == null) return null;

            T instance = (T)_instance;
            instance.Entity = this;
            components.Add(instance);

            instance.Load();

            return instance;
        }

        public bool RemoveComponent<T>() where T : EntityComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    components[i].enabled = false;
                    components.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool HasComponent<T>() where T : EntityComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    return true;
                }
            }
            return false;
        }

        public T? GetComponent<T>() where T : EntityComponent
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

        public bool GetComponent<T>(out T component) where T : EntityComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    component = (T)components[i];
                    return true;
                }
            }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            component = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return false;
        }

        public void OnCursorEnter()
        {
            foreach (EntityComponent component in components)
            {
                component.OnMouseEnter();
            }
        }
        public void OnCursorStay(float time)
        {
            foreach (EntityComponent component in components)
            {
                component.OnMouseStay(time);
            }
        }
        public void OnCursorExit()
        {
            foreach (EntityComponent component in components)
            {
                component.OnMouseExit();
            }
        }
        public void OnCursorDown(MouseButton mouseButton)
        {
            foreach (EntityComponent component in components)
            {
                component.OnMouseDown(mouseButton);
            }
        }
        public void OnCursorHold(MouseButton mouseButton, float time)
        {
            foreach (EntityComponent component in components)
            {
                component.OnMouseHold(mouseButton, time);
            }
        }
        public void OnCursorUp()
        {
            foreach (EntityComponent component in components)
            {
                component.OnMouseUp();
            }
        }
    }
}
