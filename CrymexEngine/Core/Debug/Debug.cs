using System.Diagnostics;
using System.Text;

namespace CrymexEngine
{
    public static class Debug
    {
        public static readonly string assetsPath = Directory.GetCurrentDirectory() + "\\Assets\\";
        public static readonly string runtimeAssetsPath = Directory.GetCurrentDirectory() + "\\Precompiled\\";
        public static readonly string logFolderPath = Directory.GetCurrentDirectory() + "\\Logs\\";
        public static readonly string saveFolderPath = Directory.GetCurrentDirectory() + "\\Saved\\";

        public static bool logToFile { get; private set; }

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

            logFileStream = File.Create($"{logFolderPath}{Time.CurrentDateTimeShortString}.log");
        }

        public static void Log(object? message)
        {
            if (!Engine.debugMode) return;

            LogToFile(message, LogSeverity.Message);

            WriteToConsole(message, ConsoleColor.White);
        }
        public static void Log(object? message, ConsoleColor color)
        {
            if (!Engine.debugMode) return;

            LogToFile(message, LogSeverity.Message);

            WriteToConsole(message, color);
        }

        public static void LogError(object? message)
        {
            if (!Engine.debugMode) return;

            LogToFile(message, LogSeverity.Error);

            WriteToConsole(message, ConsoleColor.DarkRed);
        }

        public static void LogWarning(object? message)
        {
            if (!Engine.debugMode) return;

            LogToFile(message, LogSeverity.Warning);

            WriteToConsole(message, ConsoleColor.Yellow);
        }

        public static void LogStatus(object? message)
        {
            if (!Engine.debugMode) return;

            LogToFile(message, LogSeverity.Status);

            WriteToConsole(message, ConsoleColor.Blue);
        }

        public static void LogToFile(object? message, LogSeverity severity)
        {
            if (!logToFile || logFileStream == null || !logFileStream.CanWrite || message == null || !Engine.debugMode) return;

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
            if (!Engine.debugMode) return;

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string DoubleToShortString(double value)
        {
            string rawString = value.ToString();

            string[] split = rawString.Split('.');
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
    }

    public enum LogSeverity { Message, Status, Warning, Error, Custom }
}
