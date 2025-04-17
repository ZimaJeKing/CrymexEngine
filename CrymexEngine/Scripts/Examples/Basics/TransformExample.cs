using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class TransformExample : ScriptableBehaviour
    {
        private Entity box;

        protected override void Load()
        {
            box = new Entity(Texture.White, Vector2.Zero, new Vector2(64));

            box.Renderer.color = Color4.Purple;

            Entity child0 = new Entity(Texture.White, new Vector2(128, 0), new Vector2(64), box, "Child0");
            child0.Renderer.color = Color4.Red;
            child0.AddComponent<LocalSinePosition>().direction = new Vector2(-1, 0);   

            Entity child1 = new Entity(Texture.White, new Vector2(-128, 0), new Vector2(64), box, "Child1");
            child1.Renderer.color = Color4.Blue;
            child1.AddComponent<LocalSinePosition>().direction = new Vector2(1, 0);

            Entity child2 = new Entity(Texture.White, new Vector2(0, 128), new Vector2(64), box, "Child2");
            child2.Renderer.color = Color4.Green;
            child2.AddComponent<LocalSinePosition>().direction = new Vector2(0, -1);

            Entity child3 = new Entity(Texture.White, new Vector2(0, -128), new Vector2(64), box, "Child3");
            child3.Renderer.color = Color4.Yellow;
            child3.AddComponent<LocalSinePosition>().direction = new Vector2(0, 1);
        }

        protected override void Update()
        {
            box.Transform.Rotation = Time.GameTime * 30;
        }
    }

    public class LocalSinePosition : EntityComponent
    {
        public Vector2 direction;

        public override void PreRender()
        {
        }

        protected override void Load()
        {
        }

        protected override void Update()
        {
            float value = MathF.Sin(Time.GameTime * 10) * 64 * Time.DeltaTime;
            entity.Transform.LocalPosition += direction * value;
        }
    }
}
