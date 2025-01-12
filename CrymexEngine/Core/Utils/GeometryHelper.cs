using OpenTK.Mathematics;

namespace CrymexEngine.Utils
{
    public static class GeometryHelper
    {
        public static Vector2 GetVertexPosition(QuadVertex vertex, Vector2 center, float width, float height, float rotation = 0)
        {

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            Vector2 localVertex = vertex switch
            {
                QuadVertex.BottomLeft => new Vector2(-halfWidth, -halfHeight),
                QuadVertex.BottomRight => new Vector2(halfWidth, -halfHeight),
                QuadVertex.TopRight => new Vector2(halfWidth, halfHeight),
                QuadVertex.TopLeft => new Vector2(-halfWidth, halfHeight),
                _ => throw new ArgumentOutOfRangeException("vertex", "Out of range of the QuadVertex enum")
            };

            if (rotation == 0)
            {
                return center + localVertex;
            }

            // Apply rotation
            float radians = MathHelper.DegreesToRadians(rotation);
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);
            Vector2 rotatedVertex = new Vector2(
                cos * localVertex.X - sin * localVertex.Y,
                sin * localVertex.X + cos * localVertex.Y
            );

            return rotatedVertex + center;
        }

        public static bool CollidersOverlap(Collider a, Collider b)
        {
            Type typeofA = a.GetType();
            Type typeofB = b.GetType();

            Type circleColliderType = typeof(CircleCollider);
            Type boxColliderType = typeof(BoxCollider);

            if (typeofA == boxColliderType && typeofB == boxColliderType) // Two BoxColliders
            {
                return TwoBoxesOverlap((BoxCollider)a, (BoxCollider)b);
            }
            else if (typeofA == circleColliderType && typeofB == circleColliderType) // Two CircleColliders
            {
                return TwoCirclesOverlap((CircleCollider)a, (CircleCollider)b);
            }
            else if (typeofA == boxColliderType && typeofB == circleColliderType) // BoxCollider A and CircleCollider B
            {
                return BoxAndCircleOverlap((BoxCollider)a, (CircleCollider)b);
            }
            else if (typeofA == circleColliderType && typeofB == boxColliderType) // BoxCollider A and CircleCollider B
            {
                return BoxAndCircleOverlap((BoxCollider)b, (CircleCollider)a);
            }
            return false;
        }

        public static Vector2 RotatePoint(Vector2 point, float angle)
        {
            float cosAngle = MathF.Cos(angle);
            float sinAngle = MathF.Sin(angle);

            float xNew = point.X * cosAngle - point.Y * sinAngle;
            float yNew = point.X * sinAngle + point.Y * cosAngle;

            return new Vector2(xNew, yNew);
        }

        private static bool TwoCirclesOverlap(CircleCollider a, CircleCollider b)
        {
            if (MathF.Pow(a.radius + b.radius, 2) > Vector2.DistanceSquared(a.entity.Position + a.offset, b.entity.Position + b.offset)) return true;
            else return false;
        }

        private static bool TwoBoxesOverlap(BoxCollider a, BoxCollider b)
        {
            Vector2[] vertices1 = GetQuadVertices(a.entity.Position + a.offset, a.size, a.entity.Rotation);
            Vector2[] vertices2 = GetQuadVertices(b.entity.Position + b.offset, b.size, b.entity.Rotation);

            Vector2[] axes = GetQuadSeparatingAxes(vertices1, vertices2);

            foreach (var axis in axes)
            {
                if (!IsOverlapOnAxis(axis, vertices1, vertices2))
                {
                    return false; // Separating axis found, no overlap
                }
            }

            return true; // Overlap on all axes
        }

        private static bool BoxAndCircleOverlap(BoxCollider a, CircleCollider b)
        {
            if (a.GetClosestPoint(b.entity.Position + b.offset) <= b.radius)
            {
                return true;
            }
            return false;
        }

        private static Vector2[] GetQuadVertices(Vector2 center, Vector2 size, float rotation = 0)
        {
            float halfWidth = size.X / 2;
            float halfHeight = size.Y / 2;

            // Define the local vertices relative to the center
            Vector2[] localVertices = new Vector2[]
            {
                new Vector2(-halfWidth, -halfHeight),
                new Vector2(halfWidth, -halfHeight),
                new Vector2(halfWidth, halfHeight),
                new Vector2(-halfWidth, halfHeight)
            };

            // Rotate and translate vertices
            float radians = MathHelper.DegreesToRadians(rotation);
            Vector2[] worldVertices = new Vector2[4];
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            for (int i = 0; i < 4; i++)
            {
                worldVertices[i] = new Vector2(
                    cos * localVertices[i].X - sin * localVertices[i].Y,
                    sin * localVertices[i].X + cos * localVertices[i].Y
                ) + center;
            }

            return worldVertices;
        }

        private static Vector2[] GetQuadSeparatingAxes(Vector2[] vertices1, Vector2[] vertices2)
        {
            Vector2[] axes = new Vector2[4];
            for (int i = 0; i < 2; i++)
            {
                Vector2[] vertices = i == 0 ? vertices1 : vertices2;
                for (int j = 0; j < 2; j++)
                {
                    int next = (j + 1) % 4;
                    Vector2 edge = vertices[next] - vertices[j];
                    axes[i * 2 + j] = new Vector2(-edge.Y, edge.X); // Perpendicular vector
                }
            }

            return axes;
        }

        private static (float min, float max) ProjectVertices(Vector2 axis, Vector2[] vertices)
        {
            float min = Vector2.Dot(axis, vertices[0]);
            float max = min;

            for (int i = 1; i < vertices.Length; i++)
            {
                float projection = Vector2.Dot(axis, vertices[i]);
                if (projection < min) min = projection;
                if (projection > max) max = projection;
            }

            return (min, max);
        }

        private static bool IsOverlapOnAxis(Vector2 axis, Vector2[] vertices1, Vector2[] vertices2)
        {
            (float min1, float max1) = ProjectVertices(axis, vertices1);
            (float min2, float max2) = ProjectVertices(axis, vertices2);

            // Check for overlap
            return !(max1 < min2 || max2 < min1);
        }
    }

    public enum QuadVertex { TopLeft, TopRight, BottomLeft, BottomRight }
}
