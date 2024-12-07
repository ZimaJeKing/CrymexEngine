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
            private set
            {
                _active = value;
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
            private set
            {
                _memoryUsage = value;
            }
        }
        public static long TextureMemoryUsage
        {
            get
            {
                return _textureMemoryUsage;
            }
            private set
            {
                _textureMemoryUsage = value;
            }
        }
        public static long AudioMmeoryUsage
        {
            get
            {
                return _audioMemoryUsage;
            }
            private set
            {
                _audioMemoryUsage = value;
            }
        }

        /// <summary>
        /// Processor time in miliseconds
        /// </summary>
        public static long ProcessorTime
        {
            get
            {
                return _processorTime;
            }
            private set
            {
                _processorTime = value;
            }
        }
        public static int threadCount
        {
            get
            {
                return _threadCount;
            }
            private set
            {
                _threadCount = value;
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

        public static void Init()
        {
            if (!Settings.GetSetting("UsageProfiler", out SettingOption option, SettingType.Bool)) return;

            if (!option.GetValue<bool>()) return;

            _glRenderTimeQuery = GL.GenQuery();

            LogStartupInfo();
            Active = true;
        }

        public static void UpdateStats()
        {
            if (!Active) return;

            MemoryUsage = _currentProcess.PrivateMemorySize64;
            threadCount = _currentProcess.Threads.Count;

            Debug.LogToFile(GetUsageProfile(), LogSeverity.Custom);
        }

        public static void BeginProcessorTimeQuery()
        {
            if (!Active) return;

            GL.BeginQuery(QueryTarget.TimeElapsed, _glRenderTimeQuery);
        }
        public static void EndProcessorTimeQuery()
        {
            if (!Active) return;

            GL.EndQuery(QueryTarget.TimeElapsed);
            GL.GetQueryObject(_glRenderTimeQuery, GetQueryObjectParam.QueryResult, out int nanoseconds);
            ProcessorTime = nanoseconds;
        }

        public static void AddDataConsumptionValue(int byteCount, MemoryUsageType type)
        {
            switch (type)
            {
                case MemoryUsageType.Texture:
                    {
                        TextureMemoryUsage += byteCount;
                        break;
                    }
                case MemoryUsageType.Audio:
                    {
                        AudioMmeoryUsage += byteCount;
                        break;
                    }
            }
        }

        private static string GetUsageProfile()
        {
            string profile = "\nUsage Profiler:\n";
            profile += $"Time: {Debug.shortTime}\n";
            profile += $"Ram usage: {ByteCountToString(MemoryUsage)}\n";
            profile += $"Processor time: {ProcessorTime}ns\n";
            profile += $"Thread count: {threadCount}\n";
            profile += $"Avarage FPS: {Window.FramesPerSecond}\n";
            return profile;
        }

        private static void LogStartupInfo()
        {
            Debug.LogToFile("Usage profiler startup info:", LogSeverity.Custom);

            int maxMsaaSamples;
            GL.GetInteger(GetPName.MaxSamples, out maxMsaaSamples);

            Debug.LogToFile($"Max supported MSAA samples: {maxMsaaSamples}", LogSeverity.Custom);

            Debug.LogToFile($"Texture memory usage: {ByteCountToString(TextureMemoryUsage)}", LogSeverity.Custom);
            Debug.LogToFile($"Audio memory usage: {ByteCountToString(AudioMmeoryUsage)}", LogSeverity.Custom);
            Debug.LogToFile($"Total memory usage: {ByteCountToString(_currentProcess.PrivateMemorySize64)}", LogSeverity.Custom);
        }

        /// <summary>
        /// Converts a number of bytes to a KB, MB, GB or TB string
        /// </summary>
        /// <returns></returns>
        private static string ByteCountToString(long byteCount)
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
    }

    public enum MemoryUsageType { Texture, Audio }
}
