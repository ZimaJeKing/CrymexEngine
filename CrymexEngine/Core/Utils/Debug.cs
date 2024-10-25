using System.Drawing;

namespace CrymexEngine
{
    public static class Debug
    {
        public static string assetsPath = Directory.GetCurrentDirectory() + "\\Assets\\";

        public static void Log(object message)
        {
            Console.Write(message);
        }
        public static void Log(object message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void LogL(object message)
        {
            Console.WriteLine(message);
        }
        public static void LogL(object message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
