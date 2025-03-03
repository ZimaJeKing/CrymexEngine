using CrymexEngine.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CrymexEngine
{
    public class Time
    {
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
                return DataUtil.SecondsToTimeString((float)TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan().TotalSeconds);
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

        internal static void Set(float deltaTime)
        {
            _deltaTime = deltaTime;
        }
    }
}
