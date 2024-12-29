using CrymexEngine.Scenes;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class Collider : EntityComponent
    {
        public override void Load()
        {
            Scene.current.colliders.Add(this);
        }

        public virtual float GetClosestPoint(Collider c)
        {
            if (c.GetType() == typeof(SphereCollider)) return Vector2.Distance(Entity.Position, c.Entity.Position) - ((SphereCollider)c).radius;
            return Vector2.Distance(Entity.Position, c.Entity.Position);
        }
        public virtual float GetClosestPoint(Vector2 point)
        {
            return Vector2.Distance(point, Entity.Position);
        }
    }

    public class SphereCollider : Collider
    {
        public float radius;
        public Vector2 offset;

        public override float GetClosestPoint(Collider c)
        {
            if (c.GetType() == typeof(SphereCollider)) return Vector2.Distance(Entity.Position + offset, c.Entity.Position) - ((SphereCollider)c).radius - radius;
            return Vector2.Distance(Entity.Position + offset, c.Entity.Position) - radius;
        }
        public override float GetClosestPoint(Vector2 point)
        {
            return Vector2.Distance(point, Entity.Position + offset) - radius;
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

                float dist = MathF.Sqrt(MathF.Pow(c.Entity.Position.X - Entity.Position.X, 2) + MathF.Pow(c.Entity.Position.Y - Entity.Position.Y, 2));

                float distX = dist - (size.X * 0.5f) - (bc.size.X * 0.5f);
                float distY = dist - (size.Y * 0.5f) - (bc.size.Y * 0.5f);

                return MathF.Min(distX, distY);
            }
            return -1;
        }
        public override float GetClosestPoint(Vector2 point)
        {
            float distX = MathF.Max(0, MathF.Abs(point.X - Entity.Position.X) - (size.X / 2));
            float distY = MathF.Max(0, MathF.Abs(point.Y - Entity.Position.Y) - (size.Y / 2));

            return MathF.Sqrt(MathF.Pow(distX, 2) + MathF.Pow(distY, 2));
        }
    }
}
