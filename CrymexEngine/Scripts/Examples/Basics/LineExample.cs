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
            middlePoint = new Entity(circleTexture, Vector2.Zero, new Vector2(5), null, "MiddlePoint", 1);
            middlePoint.Renderer.color = Color4.Blue;

            // Create the line itself
            line = new Line(new Vector2(-256, 0), new Vector2(256, 0), Color4.White, 5, -1);

            // Create a gradient for the line
            Gradient gradient = new Gradient(Color4.BlueViolet, Color4.BlueViolet);
            gradient.AddKeypoint(0.25f, Color4.Yellow);
            gradient.AddKeypoint(0.5f, Color4.Red);
            gradient.AddKeypoint(0.75f, Color4.Yellow);

            line.Gradient = gradient;
        }

        protected override void Update()
        {
            // Set the line positions for a sinusoidal motion
            start.Transform.Position = new Vector2(-256, MathF.Sin(Time.GameTime) * 256);
            end.Transform.Position = new Vector2(256, MathF.Cos(Time.GameTime) * 256);

            // Set the middle point's position to the line's middle point
            middlePoint.Transform.Position = line.MiddlePoint;

            // When assigning both points at the same time use this approach as it is faster
            line.StartEnd = (start.Transform.Position, end.Transform.Position);
        }
    }
}
