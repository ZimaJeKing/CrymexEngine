using CrymexEngine.Data;
using System.Text;

namespace CrymexEngine
{
    public class Debug
    {
        /// <summary>
        /// An internal instance
        /// </summary>
        public static Debug Instance => _instance;

        /// <summary>
        /// Enables or disables console output
        /// </summary>
        public static bool logToConsole;

        public static bool LogToFile => _logToFile;

        private static FileStream _logFileStream;
        private static bool _logToFile;

        private static readonly Debug _instance = new Debug();

        public void Cleanup()
        {
            if (!_logToFile) return;

            _logFileStream.Flush();
            _logFileStream.Dispose();
            _logFileStream.Close();
        }

        public void Init()
        {
            if (Settings.GetSetting("LogToConsole", out SettingOption logToConsoleSetting, SettingType.Bool))
            {
                logToConsole = logToConsoleSetting.GetValue<bool>();
                if (logToConsole) { Console.Clear(); Console.ResetColor(); }
            }

            // Create a log file
            if (Settings.GetSetting("LogToFile", out SettingOption logToFileSetting, SettingType.Bool))
            {
                _logToFile = logToFileSetting.GetValue<bool>();
            }

            if (_logToFile)
            {
                _logFileStream = File.Create($"{IO.logFolderPath}{Time.CurrentDateTimeShortString}.log");
            }
        }

        public static void Log(object? message)
        {
            if (!logToConsole) return;

            WriteToLogFile(message, LogSeverity.Message);

            WriteToConsole(message, ConsoleColor.White);
        }
        public static void Log(object? message, ConsoleColor color)
        {
            if (!logToConsole) return;

            WriteToLogFile(message, LogSeverity.Message);

            WriteToConsole(message, color);
        }

        public static void LogError(object? message)
        {
            if (!logToConsole) return;

            WriteToLogFile(message, LogSeverity.Error);

            WriteToConsole(message, ConsoleColor.DarkRed);
        }

        public static void LogWarning(object? message)
        {
            if (!logToConsole) return;

            WriteToLogFile(message, LogSeverity.Warning);

            WriteToConsole(message, ConsoleColor.Yellow);
        }

        public static void LogStatus(object? message)
        {
            if (!logToConsole) return;

            WriteToLogFile(message, LogSeverity.Status);

            WriteToConsole(message, ConsoleColor.Blue);
        }

        public static void WriteToLogFile(object? message, LogSeverity severity)
        {
            if (!_logToFile || _logFileStream == null || !_logFileStream.CanWrite || message == null || !logToConsole) return;

            string? msgString = message.ToString();
            if (msgString == null) return;

            if (severity == LogSeverity.Custom)
            {
                _logFileStream.Write(Encoding.Unicode.GetBytes(msgString + '\n'));
                _logFileStream.Flush();
                return;
            }

            _logFileStream.Write(Encoding.Unicode.GetBytes($"\n{Time.CurrentTimeString} | {severity} | {msgString}\n"));
            _logFileStream.Flush();
        }

        public static void WriteToConsole(object? message, ConsoleColor color)
        {
            if (!logToConsole) return;

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    public enum LogSeverity { Message, Status, Warning, Error, Custom }
}
