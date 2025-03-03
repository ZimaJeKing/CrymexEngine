using System.Diagnostics;
using CrymexEngine.Utils;
using OpenTK.Graphics.OpenGL;

namespace CrymexEngine.Debugging
{
    public class UsageProfiler
    {
        public static bool Active
        {
            get
            {
                return _active;
            }
        }

        /// <summary>
        /// Ram usage in byteCount
        /// </summary>
        public static long MemoryUsage
        {
            get
            { 
                return _memoryUsage; 
            }
        }
        public static long TextureMemoryUsage
        {
            get
            {
                return _textureMemoryUsage;
            }
        }
        public static long AudioMmeoryUsage
        {
            get
            {
                return _audioMemoryUsage;
            }
        }

        /// <summary>
        /// Processor time in nanoseconds
        /// </summary>
        public static float ProcessorTime
        {
            get
            {
                return _processorTime;
            }
        }
        public static int ThreadCount
        {
            get
            {
                return _threadCount;
            }
        }

        private static Process _currentProcess = Process.GetCurrentProcess();

        private static bool _active;
        private static long _memoryUsage;
        private static long _textureMemoryUsage;
        private static long _audioMemoryUsage;
        private static long _otherMemoryUsage;
        private static int _threadCount;
        private static float _processorTime;
        private static float _processorTimerStart;
        private static int _processorTimeFrameCount;
        private static float _processorTimeSum;

        internal static void Init()
        {
            if (Window.Loaded) return;

            if (!Settings.GlobalSettings.GetSetting("UsageProfiler", out SettingOption option, SettingType.Bool) || !option.GetValue<bool>()) return;

            Debug.LogLocalInfo("Usage Profiler", "Usage profiler active");
            LogStartupInfo();
            _active = true;
        }

        internal static void UpdateStats()
        {
            if (!Active) return;

            _memoryUsage = _currentProcess.PrivateMemorySize64;
            _threadCount = _currentProcess.Threads.Count;

            _processorTime = _processorTimeSum / _processorTimeFrameCount;
            _processorTimeFrameCount = 0;
            _processorTimeSum = 0;

            Debug.WriteToLogFile(GetUsageProfileLog(), LogSeverity.Custom);
        }

        internal static void BeginProcessorTimeQuery()
        {
            if (!Active) return;

            _processorTimerStart = Time.GameTime;
            _processorTimeFrameCount++;
        }
        internal static void EndProcessorTimeQuery()
        {
            if (!Active) return;

            _processorTimeSum += Time.GameTime - _processorTimerStart;
        }

        public static void AddMemoryConsumptionValue(int byteCount, MemoryUsageType type)
        {
            switch (type)
            {
                case MemoryUsageType.Texture:
                    {
                        _textureMemoryUsage += byteCount;
                        break;
                    }
                case MemoryUsageType.Audio:
                    {
                        _audioMemoryUsage += byteCount;
                        break;
                    }
                case MemoryUsageType.Other:
                    {
                        _otherMemoryUsage += byteCount;
                        break;
                    }
            }
        }

        private static string GetUsageProfileLog()
        {
            string profile = "\nUsage Profile:\n";
            profile += $"Time: {Time.CurrentTimeString}\n";
            profile += $"Ram usage: {DataUtil.ByteCountToString(MemoryUsage)}\n";
            profile += $"Avarage processor time: {DataUtil.FloatToShortString(ProcessorTime * 1000, 2)}ms\n";
            profile += $"Thread count: {ThreadCount}\n";
            profile += $"FPS: {Window.FramesPerSecond}";
            return profile;
        }

        private static void LogStartupInfo()
        {
            Debug.WriteToConsole($"Memory usage: (Usage Profiler)", ConsoleColor.DarkGreen);
            Debug.WriteToConsole($" > Textures: {DataUtil.ByteCountToString(_textureMemoryUsage)}", ConsoleColor.DarkGreen);
            Debug.WriteToConsole($" > Audio: {DataUtil.ByteCountToString(_audioMemoryUsage)}", ConsoleColor.DarkGreen);
            Debug.WriteToConsole($" > Other: {DataUtil.ByteCountToString(_otherMemoryUsage)}", ConsoleColor.DarkGreen);
            Debug.WriteToConsole($"   > Total: {DataUtil.ByteCountToString(_currentProcess.PrivateMemorySize64)}", ConsoleColor.DarkGreen);
        }
    }

    public enum MemoryUsageType { Other, Texture, Audio }
}
