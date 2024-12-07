using System.Diagnostics;
using System.Text;

namespace CrymexEngine
{
    public static class Debug
    {
        public static string assetsPath = Directory.GetCurrentDirectory() + "\\Assets\\";

        public static string shortTime
        {
            get
            {
                return $"{DateTime.Now.Hour}h:{DateTime.Now.Minute}m:{DateTime.Now.Second}s:{DateTime.Now.Millisecond}ms";
            }
        }

        public static bool logToFile { get; private set; }

        private static string logFolderPath = Directory.GetCurrentDirectory() + "\\Logs\\";
        private static FileStream logFileStream;

        /// <summary>
        /// A function to close
        /// </summary>
        public static void Cleanup()
        {
            if (!logToFile) return;

            logFileStream.Flush();
            logFileStream.Dispose();
            logFileStream.Close();
        }

        public static void Init()
        {
            if (Settings.GetSetting("LogToFile", out SettingOption logToFileSetting, SettingType.Bool))
            {
                logToFile = logToFileSetting.GetValue<bool>();
            }

            if (!logToFile) return;

            DateTime now = DateTime.Now;
            logFileStream = File.Create(logFolderPath + now.Day + now.Month + now.Year + ' ' + now.Hour + '_' + now.Minute + '_' + now.Second + ".log");
        }

        public static void Log(object message)
        {
            if (!Application.debugMode) return;

            LogToFile(message, LogSeverity.Message);

            WriteToConsole(message, ConsoleColor.White);
        }
        public static void Log(object message, ConsoleColor color)
        {
            if (!Application.debugMode) return;

            LogToFile(message, LogSeverity.Message);

            WriteToConsole(message, color);
        }

        public static void LogError(object message)
        {
            if (!Application.debugMode) return;

            LogToFile(message, LogSeverity.Error);

            WriteToConsole(message, ConsoleColor.DarkRed);
        }

        public static void LogWarning(object message)
        {
            if (!Application.debugMode) return;

            LogToFile(message, LogSeverity.Warning);

            WriteToConsole(message, ConsoleColor.Yellow);
        }

        public static void LogStatus(object message)
        {
            if (!Application.debugMode) return;

            LogToFile(message, LogSeverity.Status);

            WriteToConsole(message, ConsoleColor.Blue);
        }

        public static void LogToFile(object message, LogSeverity severity)
        {
            if (!logToFile || logFileStream == null || !logFileStream.CanWrite) return;

            string? msgString = message.ToString();
            if (msgString == null) return;

            if (severity == LogSeverity.Custom)
            {
                logFileStream.Write(Encoding.Unicode.GetBytes(msgString + '\n'));
                logFileStream.Flush();
                return;
            }

            logFileStream.Write(Encoding.Unicode.GetBytes($"{shortTime} | {severity} | {msgString}\n"));
            logFileStream.Flush();
        }

        private static void WriteToConsole(object message, ConsoleColor color)
        {
            if (!Application.debugMode) return;

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public enum LogSeverity { Message, Status, Warning, Error, Custom }
}
