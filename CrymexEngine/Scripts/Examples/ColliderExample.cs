using CrymexEngine.Utils;
using OpenTK.Mathematics;

namespace CrymexEngine.Examples
{
    public class ColliderExample : ScriptableBehaviour
    {
        private Entity cursorColliderEntity;
        private BoxCollider cursorCollider;
        private Collider[] colliders = new Collider[64];

        // Made for a 512x512 window
        protected override void Load()
        {
            // Cursor collider
            cursorColliderEntity = new Entity(Texture.White, Vector2.Zero, new Vector2(100));
            cursorCollider = cursorColliderEntity.AddComponent<BoxCollider>();
            cursorCollider.size = new Vector2(100);
            cursorColliderEntity.Renderer.Depth = -50;

            // 32 box colliders
            for (int i = 0; i < 32; i++)
            {
                Entity colliderEntity = new Entity(Texture.White, new Vector2(Random.Range(-256f, 256f), Random.Range(-256f, 256f)), new Vector2(Random.Range(10, 50)));
                BoxCollider collider = colliderEntity.AddComponent<BoxCollider>();
                collider.size = colliderEntity.Scale;
                colliders[i] = collider;
            }

            // 32 circle colliders
            Texture circleTexture = Assets.GetTexture("Circle");
            for (int i = 32; i < 64; i++)
            {
                Entity colliderEntity = new Entity(circleTexture, new Vector2(Random.Range(-256f, 256f), Random.Range(-256f, 256f)), new Vector2(Random.Range(10, 50)));
                CircleCollider collider = colliderEntity.AddComponent<CircleCollider>();
                collider.radius = colliderEntity.HalfScale.X;
                colliders[i] = collider;
            }
        }

        protected override void Update()
        {
            if (Input.MouseScrollDelta.Y != 0)
            {
                cursorColliderEntity.Rotation += Input.MouseScrollDelta.Y * 10;
            }

            foreach (Collider collider in colliders)
            {
                if (GeometryHelper.CollidersOverlap(cursorCollider, collider))
                {
                    if (collider.entity.Renderer.color != Color4.Red)
                    {
                        collider.entity.Renderer.color = Color4.Red;
                    }
                }
                else if (collider.entity.Renderer.color != Color4.White)
                {
                    collider.entity.Renderer.color = Color4.White;
                }
            }

            cursorColliderEntity.Position = Camera.ScreenSpaceToWorldSpace(Input.MousePosition);

            // Random color changing
            float colorValue = Time.GameTime % 1;
            cursorColliderEntity.Renderer.color = new Color4(colorValue, MathF.Abs(MathF.Sin(colorValue * 4)), 1 - colorValue, 1);
        }
    }
}
