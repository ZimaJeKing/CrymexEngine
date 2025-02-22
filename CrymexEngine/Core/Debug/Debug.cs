using CrymexEngine.Data;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
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
            if (Settings.GlobalSettings.GetSetting("LogToConsole", out SettingOption logToConsoleSetting, SettingType.Bool) && logToConsoleSetting.GetValue<bool>())
            {
                logToConsole = true;
                Console.Clear(); 
                Console.ResetColor(); 
            }

            if (Settings.GlobalSettings.GetSetting("AdditionalDebugInfo", out SettingOption additionalDebugInfoSetting, SettingType.Bool) && additionalDebugInfoSetting.GetValue<bool>())
            {
                _logAdditionalInfo = true;
            }

            if (Settings.GlobalSettings.GetSetting("LogToFile", out SettingOption logToFileSetting, SettingType.Bool) && logToFileSetting.GetValue<bool>())
            {
                _logToFile = true;
                _logFileStream = File.Create($"{Directories.LogFolderPath}{Time.CurrentDateTimeShortString}.log");
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

        internal static void LogGLDebugInfo(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            // Read the message
            string? msgString = string.Empty;
            if (message != IntPtr.Zero)
            {
                msgString = Marshal.PtrToStringAnsi(message, length);
                if (string.IsNullOrEmpty(msgString)) return;
            }

            ConsoleColor color = GLDebugSeverityToConsoleColor(severity);
            string typeString = GLDebugTypeToString(type);
            string sourceString = GLDebugSourceToString(source);

            Log($"[OpenGL {sourceString}] {typeString}: {msgString}", color);
        }

        private static ConsoleColor GLDebugSeverityToConsoleColor(DebugSeverity severity)
        {
            switch (severity)
            {
                case DebugSeverity.DontCare: return ConsoleColor.Gray;
                case DebugSeverity.DebugSeverityNotification: return ConsoleColor.Cyan;
                case DebugSeverity.DebugSeverityLow: return ConsoleColor.White;
                case DebugSeverity.DebugSeverityMedium: return ConsoleColor.DarkYellow;
                case DebugSeverity.DebugSeverityHigh: return ConsoleColor.Red;
                default: return ConsoleColor.White;
            }
        }
        private static string GLDebugTypeToString(DebugType type)
        {
            string typeString = type.ToString();

            // Cut out the DebugType from the name
            if (typeString.Contains("DebugType")) typeString = typeString.Substring(9);

            return typeString;
        }
        private static string GLDebugSourceToString(DebugSource source)
        {
            string sourceString = source.ToString();

            // Cut out the DebugType from the name
            if (sourceString.Contains("DebugSource")) sourceString = sourceString.Substring(11);

            return sourceString;
        }

        internal static void InitializeEngineDirectories()
        {
            if (!Directory.Exists(Directories.AssetsPath)) Directory.CreateDirectory(Directories.AssetsPath);
            if (!Directory.Exists(Directories.RuntimeAssetsPath)) Directory.CreateDirectory(Directories.RuntimeAssetsPath);
            if (!Directory.Exists(Directories.LogFolderPath)) Directory.CreateDirectory(Directories.LogFolderPath);
            if (!Directory.Exists(Directories.SaveFolderPath)) Directory.CreateDirectory(Directories.SaveFolderPath);
        }
    }

    public enum LogSeverity { Message, Warning, Error, Custom }
}
