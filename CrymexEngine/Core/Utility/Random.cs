using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class Random
    {
        public static int Seed
        {
            get => _seed;
            set => _seed = value;
        }

        /// <summary>
        /// Randomly returns true or false
        /// </summary>
        public static bool Condition
        {
            get
            {
                if (_random.NextSingle() < 0.5f) return false;
                return true;
            }
        }

        /// <summary>
        /// Returns a random floating point number between inclusive 0.0 and exclusive 1.0
        /// </summary>
        public static float Value
        {
            get
            {
                return _random.NextSingle();
            }
        }

        private static int _seed = (int)DateTime.Now.ToBinary();
        private static System.Random _random = new System.Random(_seed);

        public static float Range(float min, float max) => min + (Value * (max - min));

        /// <summary>
        /// Returns a number higher or equal to min and lower than max
        /// </summary>
        public static int Range(int min, int max) => _random.Next(min, max);

        /// <summary>
        /// Returns true depending on the chance
        /// </summary>
        /// <param name="value">A number between 0 and 1 specifying the chance</param>
        public static bool Chance(float value)
        {
            if (Value < value) return true;
            return false;
        }

        public static Color4 ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax)
        {
            hueMin = Math.Clamp(hueMin, 0, 1f);
            hueMax = Math.Clamp(hueMax, 0, 1f);
            saturationMin = Math.Clamp(saturationMin, 0, 1f);
            saturationMax = Math.Clamp(saturationMax, 0, 1f);
            valueMin = Math.Clamp(valueMin, 0, 1f);
            valueMax = Math.Clamp(valueMax, 0, 1f);

            return Color4.FromHsv(new Vector4(Range(hueMin, hueMax), Range(saturationMin, saturationMax), Range(valueMin, valueMax), 1f));
        }

        public static Color4 Color(float rMin, float rMax, float gMin, float gMax, float bMin, float bMax)
        {
            rMin = Math.Clamp(rMin, 0, 1f);
            rMax = Math.Clamp(rMax, 0, 1f);
            gMin = Math.Clamp(gMin, 0, 1f);
            gMax = Math.Clamp(gMax, 0, 1f);
            bMin = Math.Clamp(bMin, 0, 1f);
            bMax = Math.Clamp(bMax, 0, 1f);

            return new Color4(Range(rMin, rMax), Range(gMin, gMax), Range(bMin, bMax), 1);
        }

        public static Color4 Gray(float min, float max)
        {
            min = Math.Clamp(min, 0, 1f);
            max = Math.Clamp(max, 0, 1f);

            float val = Range(min, max);
            return new Color4(val, val, val, 1);
        }

        /// <summary>
        /// Returns a random normalized 2D vector
        /// </summary>
        public static Vector2 PointOnUnitCircle()
        {
            float angle = Range(-MathF.PI, MathF.PI);

            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }

        public static Vector2 PointInsideUnitCircle()
        {
            return PointOnUnitCircle() * Value;
        }
    }
}
