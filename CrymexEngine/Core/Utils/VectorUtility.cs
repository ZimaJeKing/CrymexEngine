
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

        public static float AngleBetween(Vector2 vectorA, Vector2 vectorB)
        {
            Vector2 direction = vectorA - vectorB;

            float angleInRadians = MathF.Atan2(direction.Y, direction.X);

            float angleInDegrees = MathHelper.RadiansToDegrees(angleInRadians);

            angleInDegrees = (angleInDegrees + 360) % 360;

            return -angleInDegrees;
        }

        public static Vector3 Vec2ToVec3(Vector2 xy, float z)
        {
            return new Vector3(xy.X, xy.Y, z);
        }

        public static Vector2i RoundToInt(Vector2 xy)
        {
            return new Vector2i((int)xy.X, (int)xy.Y);
        }
    }
}
