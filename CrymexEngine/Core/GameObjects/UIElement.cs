using CrymexEngine.Scenes;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;

namespace CrymexEngine.UI
{
    public class UIElement : GameObject
    {
        public UIRenderer Renderer
        {
            get
            {
                return _renderer;
            }
        }

        private UIRenderer _renderer;

        public UIElement(Texture texture, Vector2 position, Vector2 scale, UIElement? parent = null, float depth = 0)
        {
            Parent = parent;
            Scale = scale;
            LocalPosition = position;

            _renderer = new UIRenderer(depth);
            _renderer.UIElement = this;
            _renderer.texture = texture;
            _renderer.Depth = depth;

            Scene.current.uiElements.Add(this);

            UICanvas.Instance.SortElements();
        }

        public void Update()
        {
            if (!enabled) return;

            foreach (UIComponent component in components)
            {
                if (component.enabled) component.Update();
            }
            Renderer.Update();
        }

        public T AddComponent<T>() where T : UIComponent
        {
            object? _instance = Activator.CreateInstance(typeof(T));
            if (_instance == null) return null;

            T instance = (T)_instance;
            instance.UIElement = this;
            instance.Load();

            components.Add(instance);

            return instance;
        }

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
    }
}
