﻿using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class Random
    {
        private static System.Random r = new System.Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Randomly returns true or false
        /// </summary>
        public static bool condition
        {
            get
            {
                if (r.Next() < int.MaxValue / 2) return false;
                return true;
            }
        }

        /// <summary>
        /// Returns a random floating point number between 0 and 1
        /// </summary>
        public static float value
        {
            get
            {
                return r.NextSingle();
            }
        }


        public static float Range(float min, float max)
        {
            return min + (value * (max - min));
        }
        public static int Range(int min, int max)
        {
            return r.Next(min, max);
        }

        /// <summary>
        /// value:
        /// a number between 0 and 1 specifying the chance percentage
        /// </summary>
        /// <param name="value">A number between 0 and 1 specifying the chance percentage</param>
        public static bool Chance(float value)
        {
            if (Random.value < value) return true;
            return false;
        }

        public static Color4 ColorHSV(float hueMin, float hueMax, float satMin, float satMax, float valMin, float valMax)
        {
            return Color4.FromHsv(new Vector4(Range(hueMin, hueMax), Range(satMin, satMax), Range(valMin, valMax), 1));
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
    }
}