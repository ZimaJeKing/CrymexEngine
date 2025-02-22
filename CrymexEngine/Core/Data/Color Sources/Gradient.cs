using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class Gradient
    {
        public List<Keypoint> KeyPoints
        {
            get
            {
                return _keyPoints;
            }
        }
        private List<Keypoint> _keyPoints;

        public Gradient()
        {
            _keyPoints = new List<Keypoint>();
        }
        public Gradient(List<Keypoint> keyPoints)
        {
            if (keyPoints == null)
            {
                _keyPoints = new List<Keypoint>();
                return;
            }

            _keyPoints = keyPoints;
        }
        public Gradient(Color4 startColor, Color4 endColor)
        {
            _keyPoints = [new Keypoint(0, startColor), new Keypoint(1f, endColor)];
        }

        public void AddKeypoint(float position, Color4 color)
        {
            position = Math.Clamp(position, 0, 1f);

            foreach (Keypoint keypoint in _keyPoints)
            {
                if (position == keypoint.position)
                {
                    return;
                }
            }

            _keyPoints.Add(new Keypoint(position, color));
            _keyPoints.Sort((a, b) => -b.position.CompareTo(a.position));
        }

        public Color4 GetValue(float x)
        {
            if (_keyPoints.Count == 0) return Color4.White;
            else if (_keyPoints.Count == 1) return _keyPoints[0].color;

            x = Math.Clamp(x, 0, 1f);

            int keypointIndex = 0;
            float position = _keyPoints[0].position;
            while (position <= x)
            {
                Keypoint current = _keyPoints[keypointIndex];
                Keypoint next = _keyPoints[keypointIndex + 1];
                if (keypointIndex >= _keyPoints.Count)
                {
                    return current.color;
                }
                if (x >= current.position && x < next.position)
                {
                    float localPosition = (x - current.position) / (next.position - current.position);

                    Color4 finalColor = Interpolate(current.color, next.color, localPosition);

                    return finalColor;
                }
                keypointIndex++;
                position = _keyPoints[keypointIndex].position;
            }
            return Color4.White;
        }

        private static Color4 Interpolate(Color4 colorA, Color4 colorB, float x)
        {
            float r = colorA.R + (colorB.R - colorA.R) * x;
            float g = colorA.G + (colorB.G - colorA.G) * x;
            float b = colorA.B + (colorB.B - colorA.B) * x;
            float a = colorA.A + (colorB.A - colorA.A) * x;

            return new Color4(r, g, b, a);
        }

        public class Keypoint
        {
            public readonly float position;
            public readonly Color4 color;

            public Keypoint(float position, Color4 color)
            {
                this.position = position;
                this.color = color;
            }
        }
    }
}
