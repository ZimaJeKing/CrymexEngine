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
        public static bool logToConsole = false;

        public static bool LogToFile => _logToFile;

        private static FileStream _logFileStream;
        private static bool _logToFile = false;
        private static bool _logAdditionalInfo;
        private static readonly Debug _instance = new Debug();

        internal void Cleanup()
        {
            if (!_logToFile) return;

            _logFileStream.Flush();
            _logFileStream.Dispose();
            _logFileStream.Close();
        }

        internal void LoadSettings()
        {
            if (Settings.GetSetting("LogToConsole", out SettingOption logToConsoleSetting, SettingType.Bool) && logToConsoleSetting.GetValue<bool>())
            {
                logToConsole = true;
                Console.Clear(); 
                Console.ResetColor(); 
            }

            if (Settings.GetSetting("AdditionalDebugInfo", out SettingOption additionalDebugInfoSetting, SettingType.Bool) && additionalDebugInfoSetting.GetValue<bool>())
            {
                _logAdditionalInfo = true;
            }

            if (Settings.GetSetting("LogToFile", out SettingOption logToFileSetting, SettingType.Bool) && logToFileSetting.GetValue<bool>())
            {
                _logToFile = true;
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

        internal static void LogLocalInfo(string sender, object? message)
        {
            if (!_logAdditionalInfo) return;

            Log($"[{sender}]: {message}", ConsoleColor.Blue);
        }
    }

    public enum LogSeverity { Message, Warning, Error, Custom }
}
