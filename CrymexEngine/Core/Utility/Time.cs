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

        /// <summary>
        /// Returns a string version of the current time expressed in local time.
        /// Ex. 15h : 48min : 4s : 56ms
        /// </summary>
        public static string CurrentTimeString
        {
            get
            {
                return DataUtil.SecondsToTimeString((float)TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan().TotalSeconds);
            }
        }

        /// <summary>
        /// Returns a short version of the current time and date.
        /// Format: [DAY][MONTH][YEAR] [HOUR]_[MIN]_[SEC].
        /// </summary>
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
