using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class LineRendererExample : ScriptableBehaviour
    {
        // This example shows how to draw lines using the LineRenderer class
        // It is a slow approach and it is recomended to use line groups instead
        Vector2[] points;
        Entity[] pointElements;
        int pointCount = 5;
        float radius = 256;
        Texture circleTexture;
        float lineThickness = 1;

        protected override void Load()
        {
            circleTexture = Assets.GetTexture("Circle");

            GenPoints();
        }

        protected override void Update()
        {
            if (Input.KeyDown(Key.F))
            {
                GenPoints();
            }

            // Drawing lines between generated points
            LineRenderer.DrawLines(points, Color4.Red, lineThickness, 1, false);

            // Updating line thickness with mouse scroll
            if (Input.MouseScrollDelta.Y != 0)
            {
                lineThickness += Input.MouseScrollDelta.Y;
                lineThickness = Math.Clamp(lineThickness, 1, 100);
            }
        }

        private void GenPoints()
        {
            if (pointElements != null)
            {
                for (int i = 0; i < pointElements.Length; i++)
                {
                    pointElements[i]?.Delete();
                }
            }

            pointElements = new Entity[pointCount];
            points = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                points[i] = Random.PointOnUnitCircle() * radius;
                Entity pointElement = new Entity(circleTexture, points[i], new Vector2(10), null, $"Point{i}");
                pointElement.Renderer.color = Color4.Red;
                pointElements[i] = pointElement;
            }
        }
    }
}
