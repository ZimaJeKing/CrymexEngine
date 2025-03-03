using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class LineExample : ScriptableBehaviour
    {
        private Line line;
        private Texture circleTexture;
        private Entity start, end, middlePoint;

        protected override void Load()
        {
            circleTexture = Assets.GetTexture("Circle");

            // Create start and end point indicators
            start = new Entity(circleTexture, Vector2.Zero, new Vector2(10), null, "StartPoint", 0);
            start.Renderer.color = Color4.Red;

            end = new Entity(circleTexture, Vector2.Zero, new Vector2(10), null, "EndPoint", 0);
            end.Renderer.color = Color4.Red;

            // Create a middle point indicator
            middlePoint = new Entity(circleTexture, Vector2.Zero, new Vector2(5), null, "MiddlePoint", 0);
            middlePoint.Renderer.color = Color4.Blue;

            // Create the line itself
            line = new Line(new Vector2(-256, 0), new Vector2(256, 0), Color4.Yellow);
        }

        protected override void Update()
        {
            // Set the line positions for a sinusoidal motion
            start.Position = new Vector2(-256, MathF.Sin(Time.GameTime) * 256);
            end.Position = new Vector2(256, MathF.Cos(Time.GameTime) * 256);

            // Set the middle point's position to the line's middle point
            middlePoint.Position = line.MiddlePoint;

            // When assigning both points at the same time use this approach as it is faster
            line.StartEnd = (start.Position, end.Position);
        }
    }
}
