using CrymexEngine.Scripts;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class SphereCast
    {
        public static bool Cast(Vector2 start, float angle, float width, float range, out CastHit hit)
        {
            if (range <= 0) range = float.MaxValue;
            if (width <= 0) width = 1f;

            width *= width;

            Vector2 newPoint = start;
            while (true)
            {
                Collider? c = null;
                float closestDistance = 10000;
                foreach (Collider collider in Scene.Current.colliders)
                {
                    if (!collider.enabled) continue;

                    float dist2Col = collider.GetClosestPoint(newPoint);
                    if (dist2Col < closestDistance)
                    {
                        c = collider;
                        closestDistance = dist2Col;
                    }
                }
                if (c == null) { hit = new CastHit(); return false; }

                if (Vector2.DistanceSquared(newPoint, start) > range * range) { hit = new CastHit(newPoint, null); return false; }

                newPoint = newPoint + new Vector2(MathF.Sin(MathHelper.DegreesToRadians(angle)) * closestDistance, MathF.Cos(MathHelper.DegreesToRadians(angle)) * closestDistance);

                if (closestDistance < width)
                {
                    hit = new CastHit(newPoint, c.Entity);
                    return true;
                }
            }
        }
    }

    public struct CastHit
    {
        public Vector2 point;
        public Entity entity;

        public CastHit(Vector2 point, Entity entity)
        {
            this.point = point;
            this.entity = entity;
        }
    }
}
