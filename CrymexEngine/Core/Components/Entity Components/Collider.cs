using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class Collider : Component
    {
        public override void Load()
        {
            Scene.colliders.Add(this);
        }

        public virtual float GetClosestPoint(Collider c)
        {
            if (c.GetType() == typeof(SphereCollider)) return Vector2.Distance(entity.position, c.entity.position) - ((SphereCollider)c).radius;
            return Vector2.Distance(entity.position, c.entity.position);
        }
        public virtual float GetClosestPoint(Vector2 point)
        {
            return Vector2.Distance(point, entity.position);
        }
    }

    public class SphereCollider : Collider
    {
        public float radius;
        public Vector2 offset;

        public override float GetClosestPoint(Collider c)
        {
            if (c.GetType() == typeof(SphereCollider)) return Vector2.Distance(entity.position + offset, c.entity.position) - ((SphereCollider)c).radius - radius;
            return Vector2.Distance(entity.position + offset, c.entity.position) - radius;
        }
        public override float GetClosestPoint(Vector2 point)
        {
            return Vector2.Distance(point, entity.position + offset) - radius;
        }
    }

    public class BoxCollider : Collider
    {
        public Vector2 size;
        public Vector2 offset;

        public override float GetClosestPoint(Collider c)
        {
            if (c.GetType() == typeof(BoxCollider))
            {
                BoxCollider bc = (BoxCollider)c;

                float dist = MathF.Sqrt(MathF.Pow(c.entity.position.X - entity.position.X, 2) + MathF.Pow(c.entity.position.Y - entity.position.Y, 2));

                float distX = dist - (size.X * 0.5f) - (bc.size.X * 0.5f);
                float distY = dist - (size.Y * 0.5f) - (bc.size.Y * 0.5f);

                return MathF.Min(distX, distY);
            }
            return -1;
        }
        public override float GetClosestPoint(Vector2 point)
        {
            float distX = MathF.Max(0, MathF.Abs(point.X - entity.position.X) - (size.X / 2));
            float distY = MathF.Max(0, MathF.Abs(point.Y - entity.position.Y) - (size.Y / 2));

            return MathF.Sqrt(MathF.Pow(distX, 2) + MathF.Pow(distY, 2));
        }
    }
}
