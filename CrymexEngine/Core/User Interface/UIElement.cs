using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public class UIElement : GameObject
    {
        private UIRenderer renderer;
        private List<UIComponent> components = new List<UIComponent>();

        public UIElement(Vector4 padding, Vector2 scale, UIElement? parent = null)
        {
            Scale = scale;

            renderer = AddComponent<UIRenderer>();
        }

        public void Update()
        {
            foreach (UIComponent component in components)
            {
                if (component.enabled) component.Update();
            }
        }

        public T AddComponent<T>() where T : UIComponent
        {
            object? _instance = Activator.CreateInstance(typeof(T));
            if (_instance == null) return null;

            T instance = (T)_instance;
            instance.UIElement = this;
            instance.renderer = renderer;
            instance.Load();

            components.Add(instance);

            return instance;
        }

        public bool RemoveComponent<T>() where T : UIComponent
        {
            for (int i = 0; i < components.Count; i++)
            {
                UIComponent c = components[i];
                if (components[i].GetType() == typeof(T))
                {
                    c.UIElement = null;
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
