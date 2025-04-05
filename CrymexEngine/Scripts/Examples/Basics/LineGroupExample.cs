using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class LineGroupExample : ScriptableBehaviour
    {
        static readonly int pointCount = 10;

        LineGroup lineGroup;
        Vector2[] points = new Vector2[pointCount];
        Entity[] pointEntities = new Entity[pointCount];
        Texture circleTexture = Assets.GetTexture("Circle");

        protected override void Load()
        {
            GenerateLineGroup();
        }

        protected override void Update()
        {
            if (Input.KeyDown(Key.F))
            {
                lineGroup.Delete();

                GenerateLineGroup();
            }

            lineGroup.Color = new Color4((MathF.Sin(Time.GameTime) + 1) * 0.5f, 0, 0, 1);
        }

        private void GenerateLineGroup()
        {
            for (int i = 0; i < points.Length; i++)
            {
                // Generate the point
                Vector2 point = Random.PointOnUnitCircle() * 256;
                points[i] = point;

                // Generate point graphics
                if (pointEntities[i] == null)
                {
                    pointEntities[i] = new Entity(circleTexture, point, new Vector2(10));
                    pointEntities[i].Renderer.color = Color4.Red;
                }
                else pointEntities[i].Transform.Position = point;
            }

            lineGroup = new LineGroup(points, Color4.Red, 2);
        }
    }
}
