using CrymexEngine.GameObjects;
using CrymexEngine.Scenes;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;

namespace CrymexEngine.UI
{
    public class UIElement : GameObject
    {
        public UIRenderer Renderer => _renderer;

        private readonly UIRenderer _renderer;

        public UIElement(Texture texture, Vector2 position, Vector2 scale, UIElement? parent = null, string name = "", float depth = 0) : base(position, scale)
        {
            if (parent != null) Transform.Parent = parent.Transform;
            this.name = name;

            _renderer = new UIRenderer(depth)
            {
                Element = this,
                texture = texture,
                Depth = depth
            };

            Scene.Current.uiElements.Add(this);

            UICanvas.Instance.SortElements();
        }

        protected override void Update()
        {
            if (!enabled) return;

            foreach (Component component in components)
            {
                if (component.enabled)
                {
                    Behaviour.UpdateBehaviour(component);
                }
            }
            if (_renderer.enabled) Behaviour.UpdateBehaviour(_renderer);
        }

        protected override void PreRender()
        {
            if (!enabled) return;

            foreach (Component component in components)
            {
                if (component.enabled) component.PreRender();
            }
        }

        /// <summary>
        /// Adds a component of the specified type to the Element
        /// </summary>
        /// <returns>The new component</returns>
        public T AddComponent<T>() where T : UIComponent
        {
            // FreeComponentAttribute check
            if (Attribute.IsDefined(typeof(T), typeof(FreeComponentAttribute)))
            {
                Debug.LogError($"Cannot add a free component to a gameobject ({typeof(T)})");
                return null;
            }

            // SingularComponentAttribute check
            if (Attribute.IsDefined(typeof(T), typeof(SingularComponentAttribute)))
            {
                if (HasComponent<T>())
                {
                    Debug.LogError($"Singular components are not designed for multiple instances on a gameobject ({typeof(T)})");
                    return null;
                }
            }

            object? _instance = Activator.CreateInstance(typeof(T));
            if (_instance == null) return null;

            if (_instance is IMouseClick _ || _instance is IMouseHover _) _handlesClickEvents = true;

            T instance = (T)_instance;
            instance.Element = this;
            Behaviour.LoadBehaviour(instance);

            components.Add(instance);

            return instance;
        }

        /// <returns>If the operation was successful</returns>
        public bool RemoveComponent<T>() where T : UIComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                UIComponent c = (UIComponent)components[i];
                if (components[i].GetType() == typeof(T))
                {
                    c.enabled = false;
                    components.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a component of a certain type. Returns null if no matching component found
        /// </summary>
        public T GetComponent<T>() where T : UIComponent
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

        public bool HasComponent<T>() where T : UIComponent
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
        /// Removes an element from the game. Happens next frame
        /// </summary>
        public override void Delete()
        {
            enabled = false;
            Scene.Current.uiElementDeleteQueue.Add(this);
        }
    }
}
