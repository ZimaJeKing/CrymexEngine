﻿using System.Diagnostics;
using CrymexEngine.Utils;

namespace CrymexEngine.Debugging
{
    public class UsageProfiler
    {
        public static bool Active => _active;
        public static Process CurrentProcess => _currentProcess;

        /// <summary>
        /// Ram usage in bytes
        /// </summary>
        public static long MemoryUsage => _memoryUsage;
        public static long TextureMemoryUsage => _textureMemoryUsage;
        public static long AudioMmeoryUsage => _audioMemoryUsage;
        public static long OtherMmeoryUsage => _otherMemoryUsage;
        public static long VideoMemoryUsage => _videoMemoryUsage;

        /// <summary>
        /// Processor time in nanoseconds
        /// </summary>
        public static float ProcessorTime => _processorTime;
        public static int ThreadCount => _threadCount;

        private static Process _currentProcess;

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
        private static long _videoMemoryUsage;

        internal static void Init()
        {
            if (Window.Loaded) return;

            if (!Settings.GlobalSettings.GetSetting("UsageProfiler", out SettingOption option, SettingType.Bool) || !option.GetValue<bool>()) return;

            _currentProcess = Process.GetCurrentProcess();
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
                case MemoryUsageType.VRam:
                    {
                        _videoMemoryUsage += byteCount;
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
            string profile = "\r\nUsage Profile:\r\n";
            profile += $"Time: {Time.CurrentTimeString}\r\n";
            profile += $"Ram usage: {DataUtil.ByteCountToString(_memoryUsage)}\r\n";
            profile += $"VRam usage: {DataUtil.ByteCountToString(_videoMemoryUsage)}\r\n";
            profile += $"Thread count: {ThreadCount}\r\n";
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

    public enum MemoryUsageType { Other, Texture, Audio, VRam }
}
