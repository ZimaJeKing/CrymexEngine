using CrymexEngine.Scripting;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class SphereCast
    {
        public static bool Cast(Vector2 start, float angle, float width, float range, out PhysicsCastHit hit)
        {
            if (Scene.current.colliders.Count == 0)
            {
                hit = new PhysicsCastHit();
                return false;
            }

            if (range <= 0f) range = float.MaxValue;
            if (width <= 0) width = 1f;

            width *= width;

            Vector2 newPoint = start;
            while (true)
            {
                // Get the closest distance from any collider
                Collider? c = null;
                float closestDistance = float.MaxValue;
                foreach (Collider collider in Scene.current.colliders)
                {
                    if (!collider.enabled) continue;

                    float dist2Col = collider.GetClosestPoint(newPoint);
                    if (dist2Col < closestDistance)
                    {
                        c = collider;
                        closestDistance = dist2Col;
                    }
                }

                // 
                if (c == null) 
                { 
                    hit = new PhysicsCastHit(); 
                    return false; 
                }

                if (Vector2.DistanceSquared(newPoint, start) > range * range) 
                { 
                    hit = new PhysicsCastHit(); 
                    return false; 
                }

                newPoint = newPoint + new Vector2(MathF.Sin(MathHelper.DegreesToRadians(angle)) * closestDistance, MathF.Cos(MathHelper.DegreesToRadians(angle)) * closestDistance);

                if (closestDistance < width)
                {
                    hit = new PhysicsCastHit(newPoint, c.Entity);
                    return true;
                }
            }
        }
    }

    public struct PhysicsCastHit
    {
        public Vector2 point;
        public Entity entity;

        public PhysicsCastHit(Vector2 point, Entity entity)
        {
            this.point = point;
            this.entity = entity;
        }
    }
}
