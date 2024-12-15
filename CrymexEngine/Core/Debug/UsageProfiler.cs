using System.Diagnostics;
using NAudio.Dmo;
using OpenTK.Graphics.OpenGL;

namespace CrymexEngine.Debugging
{
    public static class UsageProfiler
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
        public static long ProcessorTime
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
        private static long _processorTime;
        private static int _threadCount;

        private static int _glRenderTimeQuery;

        private static int _processorTimeFrameCount;
        private static long _processorTimeSum;

        public static void Init()
        {
            if (!Settings.GetSetting("UsageProfiler", out SettingOption option, SettingType.Bool)) return;

            if (!option.GetValue<bool>()) return;

            _glRenderTimeQuery = GL.GenQuery();

            LogStartupInfo();
            _active = true;
        }

        public static void UpdateStats()
        {
            if (!Active) return;

            _memoryUsage = _currentProcess.PrivateMemorySize64;
            _threadCount = _currentProcess.Threads.Count;

            _processorTime = _processorTimeSum / _processorTimeFrameCount;
            _processorTimeFrameCount = 0;
            _processorTimeSum = 0;

            Debug.LogToFile(GetUsageProfile(), LogSeverity.Custom);
        }

        public static void BeginProcessorTimeQuery()
        {
            if (!Active) return;

            _processorTimeFrameCount++;

            GL.BeginQuery(QueryTarget.TimeElapsed, _glRenderTimeQuery);
        }
        public static void EndProcessorTimeQuery()
        {
            if (!Active) return;

            GL.EndQuery(QueryTarget.TimeElapsed);
            GL.GetQueryObject(_glRenderTimeQuery, GetQueryObjectParam.QueryResult, out int nanoseconds);
            _processorTimeSum += nanoseconds;
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
            }
        }

        private static string GetUsageProfile()
        {
            string profile = "\nUsage Profiler:\n";
            profile += $"Time: {Time.CurrentTimeString}\n";
            profile += $"Ram usage: {Debug.ByteCountToString(MemoryUsage)}\n";
            profile += $"Avarage processor time: {Debug.DoubleToShortString(ProcessorTime * 0.000_001)}ms\n";
            profile += $"Thread count: {ThreadCount}\n";
            profile += $"FPS: {Window.FramesPerSecond}";
            return profile;
        }

        private static void LogStartupInfo()
        {
            Debug.LogToFile("\nUsage profiler startup info:", LogSeverity.Custom);

            int maxMsaaSamples;
            GL.GetInteger(GetPName.MaxSamples, out maxMsaaSamples);

            Debug.LogToFile($"Max supported MSAA samples: {maxMsaaSamples}", LogSeverity.Custom);

            Debug.LogToFile($"Texture memory usage: {Debug.ByteCountToString(TextureMemoryUsage)}", LogSeverity.Custom);
            Debug.LogToFile($"Audio memory usage: {Debug.ByteCountToString(AudioMmeoryUsage)}", LogSeverity.Custom);
            Debug.LogToFile($"Total memory usage: {Debug.ByteCountToString(_currentProcess.PrivateMemorySize64)}", LogSeverity.Custom);
        }
    }

    public enum MemoryUsageType { Texture, Audio }
}
