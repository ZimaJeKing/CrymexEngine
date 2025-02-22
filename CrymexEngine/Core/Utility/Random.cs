using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class Random
    {
        public static int Seed
        {
            get
            {
                return _seed;
            }
            set
            {
                _seed = value;
            }
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
        /// Returns a random floating point number between 0 and 1
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

        public static float Range(float min, float max)
        {
            return min + (Value * (max - min));
        }

        /// <summary>
        /// Returns a number higher or equal to min and lower than max
        /// </summary>
        public static int Range(int min, int max)
        {
            return _random.Next(min, max);
        }

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
            return Color4.FromHsv(new Vector4(Range(hueMin, hueMax), Range(saturationMin, saturationMax), Range(valueMin, valueMax), 1));
        }

        public static Color4 Color(float rMin, float rMax, float gMin, float gMax, float bMin, float bMax)
        {
            return new Color4(Range(rMin, rMax), Range(gMin, gMax), Range(bMin, bMax), 1);
        }

        public static Color4 Gray(float min, float max)
        {
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
    }
}
