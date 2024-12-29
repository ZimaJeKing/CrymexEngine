
using OpenTK.Mathematics;

namespace CrymexEngine.Utils
{
    public static class VectorUtility
    {
        public static Vector2 MoveTowards(Vector2 origin, Vector2 target, float distance)
        {
            Vector2 dir = target - origin;

            if (dir.Length < distance || dir == Vector2.Zero) return target;

            return origin + dir.Normalized() * distance;
        }

        public static float Angle(Vector2 origin, Vector2 target)
        {
            Vector2 dir = target - origin;
            float angle = MathF.Atan(dir.Y / dir.X);

            if (!float.IsNormal(angle)) angle = 0;

            return MathHelper.RadiansToDegrees(angle);
        }
    }
}
