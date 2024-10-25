using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public abstract class UIElement
    {
        public Vector2 margin;
        public Vector2 padding;
        public Vector2 scale;
        public UIElement? anchor;
        public AnchorPoint anchorPoint;

        protected UIElement(Vector2 margin, Vector2 padding, Vector2 scale, AnchorPoint anchorPoint, UIElement? anchor = null)
        {
            this.margin = margin;
            this.padding = padding;
            this.scale = scale;
            this.anchor = anchor;
            this.anchorPoint = anchorPoint;
        }

        abstract protected void Refresh();
    }

    public enum AnchorPoint { TopLeft, TopMiddle, TopRight, CenterLeft, CenterMiddle, CenterRight, BottomLeft, BottomMiddle, BottomRight }
}
