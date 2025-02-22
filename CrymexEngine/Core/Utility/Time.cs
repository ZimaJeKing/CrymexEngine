using CrymexEngine.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CrymexEngine
{
    public class Time
    {
        /// <summary>
        /// An internal instance
        /// </summary>
        public static Time Instance
        {
            get
            {
                return _instance;
            }
        }

        public static float GameTime
        {
            get
            {
                return (float)GLFW.GetTime();
            }
        }
        public static float DeltaTime
        {
            get
            {
                return _deltaTime;
            }
        }

        public static string CurrentTimeString
        {
            get
            {
                return DataUtilities.SecondsToTimeString((float)TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan().TotalSeconds);
            }
        }

        public static string CurrentDateTimeShortString
        {
            get
            {
                return $"{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year} {DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}";
            }
        }

        private static float _deltaTime;

        private static Time _instance = new Time();

        public void Set(float deltaTime)
        {
            _deltaTime = deltaTime;
        }
    }
}
