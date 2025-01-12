using CrymexEngine.Scenes;
using CrymexEngine.UI;
using OpenTK.Mathematics;
namespace CrymexEngine
{
    public sealed class Entity : GameObject
    {
        public Vector2 Forward
        {
            get
            {
                return new Vector2(MathF.Cos(MathHelper.DegreesToRadians(Rotation)), MathF.Sin(MathHelper.DegreesToRadians(Rotation)));
            }
        }

        public EntityRenderer? Renderer
        {
            get
            {
                return _renderer;
            }
        }

        private EntityRenderer? _renderer;

        /// <summary>
        /// Returns null, if the Entity wasn't found
        /// </summary>
        public static Entity GetEntity(string name)
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

        /// <summary>
        /// Returns null, if the Entity wasn't found
        /// </summary>
        public static Entity GetEntity(string name, Scene scene)
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

        public Entity(Texture texture, Vector2 position, Vector2 scale, Entity? parent = null, string name = "")
        {
            Parent = parent;
            LocalPosition = position;
            Scale = scale;
            Rotation = 0;

            Scene.Current.entities.Add(this);

            _renderer = AddComponent<EntityRenderer>();
            Renderer.texture = texture;
            this.name = name;
        }

        /// <summary>
        /// Creates an Entity in the specified scene
        /// </summary>
        public Entity(Texture texture, Vector2 position, Vector2 scale, Scene scene, Entity? parent = null, string name = "")
        {
            Parent = parent;
            LocalPosition = position;
            Scale = scale;
            Rotation = 0;

            scene.entities.Add(this);

            _renderer = AddComponent<EntityRenderer>();
            Renderer.texture = texture;
            this.name = name;
        }

        /// <summary>
        /// Creates an Entity with no EntityRenderer attached
        /// </summary>
        public Entity(Vector2 position, Vector2 scale, Entity? parent = null, string name = "")
        {
            Parent = parent;
            Position = position;
            Scale = scale;
            Rotation = 0;
            this.name = name;

            Scene.Current.entities.Add(this);
        }

        /// <summary>
        /// Adds a component of the specified type to the Entity
        /// </summary>
        /// <returns>The new component</returns>
        public T AddComponent<T>() where T : EntityComponent
        {
            if (Attribute.IsDefined(typeof(T), typeof(FreeComponentAttribute))) return null;

            object? _instance = Activator.CreateInstance(typeof(T));
            if (_instance == null) return null;

            if (_instance is IMouseClick _ || _instance is IMouseHover _) _handlesClickEvents = true;

            T instance = (T)_instance;
            instance.entity = this;
            components.Add(instance);

            Behaviour.LoadBehaviour(instance);

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

        /// <summary>
        /// Returns a component of a certain type. Returns null if no matching component found
        /// </summary>
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

        /// <summary>
        /// Removes an element from the game. Happens next frame
        /// </summary>
        public override void Delete()
        {
            enabled = false;
            Scene.Current.entityDeleteQueue.Add(this);
        }

        protected override void Update()
        {
            foreach (EntityComponent component in components)
            {
                if (component.enabled)
                {
                    Behaviour.UpdateBehaviour(component);
                }
            }
        }

        protected override void PreRender()
        {
            foreach (EntityComponent component in components)
            {
                if (component.enabled)
                {
                    component.PreRender();
                }
            }
        }
    }
}
