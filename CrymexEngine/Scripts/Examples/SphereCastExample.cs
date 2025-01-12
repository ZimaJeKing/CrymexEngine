using OpenTK.Mathematics;

namespace CrymexEngine.Examples
{
    public class SphereCastExample : ScriptableBehaviour
    {
        private float spherecastStepAngle = 0.1f;
        private float generationTimeStep = 0.002f;

        private float generationTimer = 0;
        private float currentAngle = 0;
        private bool generating = false;

        protected override void Load()
        {
            Camera.ClearColor = Color4.Black;

            GenerateColliders();

            Debug.Log("Starting sphere casting...");
            StartSpherecasting();
        }

        protected override void Update()
        {
            if (!generating) return;

            generationTimer += Time.DeltaTime;

            if (generationTimer < generationTimeStep) return;

            while (generationTimer > generationTimeStep)
            {
                generationTimer -= generationTimeStep;

                if (SphereCast.Cast(Vector2.Zero, currentAngle, 1, Camera.RenderDistance, out PhysicsCastHit hit))
                {
                    Entity entity = new Entity(Texture.White, hit.point, new Vector2(2));
                    entity.Renderer.color = Color4.Red;
                }

                currentAngle += spherecastStepAngle;
            }

            if (currentAngle > 360)
            {
                generating = false;
                Debug.Log("Sphere casting ended");
            }
        }

        private void StartSpherecasting() => generating = true;

        private void GenerateColliders()
        {
            // Generate box colliders
            for (int i = 0; i < 32; i++)
            {
                Entity entity = new Entity(Texture.White, Random.PointOnUnitCircle() * Window.HalfSize.X, new Vector2(Random.Range(10, 100)));
                entity.Renderer.color = new Color4(32, 32, 32, 255);

                BoxCollider collider = entity.AddComponent<BoxCollider>();
                collider.size = entity.Scale;
            }

            Texture circleTexture = Assets.GetTexture("Circle");
            // Generate circle colliders
            for (int i = 0; i < 32; i++)
            {
                Entity entity = new Entity(circleTexture, Random.PointOnUnitCircle() * Window.HalfSize.X, new Vector2(Random.Range(10, 100)));
                entity.Renderer.color = new Color4(32, 32, 32, 255);

                CircleCollider collider = entity.AddComponent<CircleCollider>();
                collider.radius = entity.HalfScale.X;
            }
        }
    }
}
