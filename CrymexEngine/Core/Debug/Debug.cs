using OpenTK.Mathematics;
using System.Text;

namespace CrymexEngine
{
    public class Debug
    {
        /// <summary>
        /// An internal instance
        /// </summary>
        public static Debug Instance
        {
            get
            {
                return _instance;
            }
        }

        public static readonly string assetsPath = Directory.GetCurrentDirectory() + "\\Assets\\";
        public static readonly string runtimeAssetsPath = Directory.GetCurrentDirectory() + "\\Precompiled\\";
        public static readonly string logFolderPath = Directory.GetCurrentDirectory() + "\\Logs\\";
        public static readonly string saveFolderPath = Directory.GetCurrentDirectory() + "\\Saved\\";

        public static bool logToConsole;

        public static bool LogToFile
        {
            get
            {
                return _logToFile;
            }
        }

        private static FileStream logFileStream;
        private static bool _logToFile;

        private static readonly Debug _instance = new Debug();

        /// <summary>
        /// A function to close
        /// </summary>
        public void Cleanup()
        {
            if (!_logToFile) return;

            logFileStream.Flush();
            logFileStream.Dispose();
            logFileStream.Close();
        }

        public void Init()
        {
            // Init directories
            if (!Directory.Exists(assetsPath)) Directory.CreateDirectory(assetsPath);
            if (!Directory.Exists(runtimeAssetsPath)) Directory.CreateDirectory(runtimeAssetsPath);
            if (!Directory.Exists(logFolderPath)) Directory.CreateDirectory(logFolderPath);
            if (!Directory.Exists(saveFolderPath)) Directory.CreateDirectory(saveFolderPath);

            if (Settings.GetSetting("LogToConsole", out SettingOption logToConsoleSetting, SettingType.Bool))
            {
                logToConsole = logToConsoleSetting.GetValue<bool>();
            }

            // Create a log file
            if (Settings.GetSetting("LogToFile", out SettingOption logToFileSetting, SettingType.Bool))
            {
                _logToFile = logToFileSetting.GetValue<bool>();
            }

            if (!_logToFile) return;

            logFileStream = File.Create($"{logFolderPath}{Time.CurrentDateTimeShortString}.log");
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
            if (!_logToFile || logFileStream == null || !logFileStream.CanWrite || message == null || !logToConsole) return;

            string? msgString = message.ToString();
            if (msgString == null) return;

            if (severity == LogSeverity.Custom)
            {
                logFileStream.Write(Encoding.Unicode.GetBytes(msgString + '\n'));
                logFileStream.Flush();
                return;
            }

            logFileStream.Write(Encoding.Unicode.GetBytes($"\n{Time.CurrentTimeString} | {severity} | {msgString}\n"));
            logFileStream.Flush();
        }

        public static void WriteToConsole(object? message, ConsoleColor color)
        {
            if (!logToConsole) return;

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string FloatToShortString(float value)
        {
            string rawString = value.ToString();

            char separator;
            if (rawString.Contains('.')) separator = '.';
            else separator = ',';

            string[] split = rawString.Split(separator);
            if (split.Length < 2 || split[1].Length < 3) return rawString;

            split[1] = split[1][0..2];
            return split[0] + '.' + split[1];
        }


        /// <summary>
        /// Converts a number of bytes to a KB, MB, GB or TB string
        /// </summary>
        public static string ByteCountToString(long byteCount)
        {
            int i = 0;
            while (byteCount > 1024)
            {
                byteCount /= 1024;
                if (byteCount < 1024)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                return $"{byteCount} KB";
                            }
                        case 1:
                            {
                                return $"{byteCount} MB";
                            }
                        case 2:
                            {
                                return $"{byteCount} GB";
                            }
                        case 3:
                            {
                                return $"{byteCount} TB";
                            }
                    }
                }
                i++;
            }
            return $"{byteCount} B";
        }

        public static int GetCheckSum(byte[] data)
        {
            if (data == null || data.Length == 0) return 0;

            int length = data.Length - (data.Length % sizeof(int));
            int[] ints = new int[length];
            Array.Copy(data, ints, length);

            int sum = 0;

            for (int i = 0; i < length; i++)
            {
                sum += ints[i];
            }

            return sum;
        }

        public static Vector3 Vec2ToVec3(Vector2 xy, float z)
        {
            return new Vector3(xy.X, xy.Y, z);
        }
    }

    public enum LogSeverity { Message, Status, Warning, Error, Custom }
}
