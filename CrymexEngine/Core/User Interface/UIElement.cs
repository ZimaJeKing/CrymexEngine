using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public class UIElement
    {
        public UIElement? anchor;
        public AnchorPoint anchorPoint;
        public FlexType flex;

        private UIRenderer renderer;
        private List<UIComponent> components = new List<UIComponent>();
        public Matrix4 scaleMatrix {  get; private set; }

        public Vector4 padding
        {
            get
            {
                return _padding;
            }
            set
            {
                _padding = value;
            }
        }
        public Vector2 position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
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
            }
        }

        public Vector4 _padding;
        public Vector2 _position;
        public Vector2 _scale;

        public Vector2 topLeftCorner
        {
            get
            {
                if (anchor == null) return CalculateAbsoluteAnchorPoint(anchorPoint, Vector2.Zero, Window.Size);
                else return CalculateAbsoluteAnchorPoint(anchorPoint, anchor.position, anchor.scale);
            }
        }

        public UIElement(Vector4 padding, Vector2 scale, AnchorPoint anchorPoint, FlexType flex = FlexType.FullStretch, UIElement? anchor = null)
        {
            this.anchorPoint = anchorPoint;
            this.flex = flex;
            this.anchor = anchor;
            this.padding = padding;
            this.scale = scale;

            renderer = AddComponent<UIRenderer>();
        }

        /// <summary>
        /// Creates an element with absolute position. Transform depends only on position and scale.
        /// </summary>
        public UIElement(Vector2 position, Vector2 scale, AnchorPoint anchorPoint, UIElement? anchor = null)
        {
            this.anchorPoint = anchorPoint;
            this.scale = scale;
            this.anchor = anchor;
            this.position = position;

            flex = FlexType.NoFlex;

            renderer = AddComponent<UIRenderer>();
        }

        public void Refresh()
        {
            foreach (UIComponent component in components)
            {
                if (component.enabled) component.Refresh();
            }
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

        // WIP
        public Vector2 CalculateFlexScale()
        {
            return Vector2.Zero;
        }

        public static Vector2 CalculateAbsoluteAnchorPoint(AnchorPoint anchorPoint, Vector2 refPosition, Vector2 refScale)
        {
            Vector2 point = refPosition;

            if (anchorPoint == AnchorPoint.TopLeft)
            {
                point += new Vector2(-refScale.X * 0.5f, refScale.Y * 0.5f);
            }
            else if (anchorPoint == AnchorPoint.TopMiddle)
            {
                point += new Vector2(refScale.X, refScale.Y * 0.5f);
            }
            else if (anchorPoint == AnchorPoint.TopRight)
            {
                point += new Vector2(refScale.X * 0.5f, refScale.Y * 0.5f);
            }
            else if (anchorPoint == AnchorPoint.CenterLeft)
            {
                point += new Vector2(-refScale.X * 0.5f, refScale.Y);
            }
            else if (anchorPoint == AnchorPoint.CenterMiddle)
            {
                point += new Vector2(refScale.X, -refScale.Y);
            }
            else if (anchorPoint == AnchorPoint.CenterRight)
            {
                point += new Vector2(refScale.X * 0.5f, -refScale.Y );
            }
            else if (anchorPoint == AnchorPoint.BottomLeft)
            {
                point += new Vector2(-refScale.X * 0.5f, -refScale.Y * 0.5f);
            }
            else if (anchorPoint == AnchorPoint.BottomMiddle)
            {
                point += new Vector2(refScale.X, -refScale.Y * 0.5f);
            }
            else if (anchorPoint == AnchorPoint.BottomRight)
            {
                point += new Vector2(refScale.X * 0.5f, -refScale.Y * 0.5f);
            }

            return point;
        }
    }

    public enum FlexType { NoFlex, FullStretch, StretchX, StretchY }
    public enum AnchorPoint { TopLeft, TopMiddle, TopRight, CenterLeft, CenterMiddle, CenterRight, BottomLeft, BottomMiddle, BottomRight }
}
