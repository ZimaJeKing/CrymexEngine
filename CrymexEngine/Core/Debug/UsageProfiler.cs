using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace CrymexEngine.Debugging
{
    public class UsageProfiler
    {
        public static UsageProfiler Instance
        {
            get
            {
                return _instance;
            }
        }

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
        private static int _threadCount;
        private static float _processorTime;
        private static float _processorTimerStart;
        private static int _processorTimeFrameCount;
        private static float _processorTimeSum;

        private static UsageProfiler _instance = new UsageProfiler();

        public void Init()
        {
            if (!Settings.GetSetting("UsageProfiler", out SettingOption option, SettingType.Bool)) return;

            if (!option.GetValue<bool>()) return;

            LogStartupInfo();
            _active = true;
        }

        public void UpdateStats()
        {
            if (!Active) return;

            _memoryUsage = _currentProcess.PrivateMemorySize64;
            _threadCount = _currentProcess.Threads.Count;

            _processorTime = _processorTimeSum / _processorTimeFrameCount;
            _processorTimeFrameCount = 0;
            _processorTimeSum = 0;

            Debug.WriteToLogFile(GetUsageProfileLog(), LogSeverity.Custom);
        }

        public void BeginProcessorTimeQuery()
        {
            if (!Active) return;

            _processorTimerStart = Time.GameTime;
            _processorTimeFrameCount++;
        }
        public void EndProcessorTimeQuery()
        {
            if (!Active) return;
            _processorTimeSum += Time.GameTime - _processorTimerStart;
        }

        public void AddMemoryConsumptionValue(int byteCount, MemoryUsageType type)
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

        private static string GetUsageProfileLog()
        {
            string profile = "\nUsage Profiler:\n";
            profile += $"Time: {Time.CurrentTimeString}\n";
            profile += $"Ram usage: {CEUtilities.ByteCountToString(MemoryUsage)}\n";
            profile += $"Avarage processor time: {CEUtilities.FloatToShortString(ProcessorTime * 1000, 2)}ms\n";
            profile += $"Thread count: {ThreadCount}\n";
            profile += $"FPS: {Window.FramesPerSecond}";
            return profile;
        }

        private static void LogStartupInfo()
        {
            Debug.WriteToLogFile("\nUsage profiler startup info:", LogSeverity.Custom);

            int maxMsaaSamples;
            GL.GetInteger(GetPName.MaxSamples, out maxMsaaSamples);

            Debug.WriteToLogFile($"Max supported MSAA samples: {maxMsaaSamples}", LogSeverity.Custom);

            Debug.WriteToLogFile($"Texture memory usage: {CEUtilities.ByteCountToString(TextureMemoryUsage)}", LogSeverity.Custom);
            Debug.WriteToLogFile($"Audio memory usage: {CEUtilities.ByteCountToString(AudioMmeoryUsage)}", LogSeverity.Custom);
            Debug.WriteToLogFile($"Total memory usage: {CEUtilities.ByteCountToString(_currentProcess.PrivateMemorySize64)}", LogSeverity.Custom);
        }
    }

    public enum MemoryUsageType { Texture, Audio }
}
