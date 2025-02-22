using nkast.Aether.Physics2D.Dynamics;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class PhysicsExample : ScriptableBehaviour
    {
        protected override void Load()
        {
            Entity groundElement = new Entity(Texture.White, new Vector2(0, -256), new Vector2(1000, 50));
            BoxCollider groundCollider = groundElement.AddComponent<BoxCollider>();
            groundCollider.Size = new Vector2(1000, 50);

            Texture circleTexture = Assets.GetTexture("Circle");

            // 10 box colliders
            for (int i = 0; i < 10; i++)
            {
                Entity colliderEntity = new Entity(Texture.White, Random.PointOnUnitCircle() * 256, new Vector2(Random.Range(10, 50), Random.Range(10, 50)));
                BoxCollider collider = colliderEntity.AddComponent<BoxCollider>();
                PhysicsBody physicsBody = colliderEntity.AddComponent<PhysicsBody>();
                collider.Size = colliderEntity.Scale;
                physicsBody.BodyType = BodyType.Dynamic;
            }

            // 10 circle colliders
            for (int i = 0; i < 10; i++)
            {
                Entity colliderEntity = new Entity(circleTexture, Random.PointOnUnitCircle() * 256, new Vector2(Random.Range(10, 50)));
                CircleCollider collider = colliderEntity.AddComponent<CircleCollider>();
                PhysicsBody physicsBody = colliderEntity.AddComponent<PhysicsBody>();
                collider.Radius = colliderEntity.HalfScale.X;
                physicsBody.BodyType = BodyType.Dynamic;
            }
        }

        protected override void Update()
        {
        }
    }
}
