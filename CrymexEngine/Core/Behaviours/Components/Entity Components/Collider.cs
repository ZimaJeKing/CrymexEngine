using CrymexEngine.Scenes;
using CrymexEngine.Utils;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class Collider : EntityComponent
    {
        protected override void Load()
        {
            Scene.Current.colliders.Add(this);
        }

        public virtual float GetClosestPoint(Vector2 point)
        {
            return Vector2.Distance(point, entity.Position);
        }
    }

    public class CircleCollider : Collider
    {
        public float radius;
        public Vector2 offset;

        public override float GetClosestPoint(Vector2 point)
        {
            return Vector2.Distance(point, entity.Position + offset) - radius;
        }
    }

    public class BoxCollider : Collider
    {
        public Vector2 size;
        public Vector2 offset;

        public override float GetClosestPoint(Vector2 point)
        {
            // Step 1: Translate the point into the rectangle's local coordinate system
            // Translate the point to the rectangle's local space by subtracting the rectangle center
            Vector2 translatedPoint = new Vector2(point.X - entity.Position.X, point.Y - entity.Position.Y);

            // Step 2: Rotate the point by the negative rotation of the rectangle to align it with axes
            Vector2 rotatedPoint = GeometryHelper.RotatePoint(translatedPoint, MathHelper.DegreesToRadians(entity.Rotation));

            // Step 3: Clamp the point to the axis-aligned rectangle's bounds
            float closestX = Math.Max(-size.X / 2, Math.Min(rotatedPoint.X, size.X / 2));
            float closestY = Math.Max(-size.Y / 2, Math.Min(rotatedPoint.Y, size.Y / 2));

            // Step 4: Calculate the Euclidean distance from the rotated point to the closest point on the rectangle
            float distanceX = rotatedPoint.X - closestX;
            float distanceY = rotatedPoint.Y - closestY;
            float closestDistance = (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            return closestDistance;
        }
    }
}
