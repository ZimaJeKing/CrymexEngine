using CrymexEngine.Debugging;
using NAudio.Wave;
using System.Runtime.InteropServices;

namespace CrymexEngine
{
    public class AudioClip : IDisposable
    {
        public readonly IntPtr soundData;
        public readonly WaveFormat format;
        public readonly int dataSize;
        public readonly float length;

        public AudioClip(IntPtr soundData, WaveFormat format, int dataSize)
        {
            this.soundData = soundData;
            this.format = format;
            this.dataSize = dataSize;
            length = dataSize / (float)(format.SampleRate * format.Channels * (format.BitsPerSample / 8f));

            UsageProfiler.AddDataConsumptionValue(dataSize, MemoryUsageType.Audio);
        }

        public static AudioClip? Load(string path)
        {
            IntPtr soundData = LoadSound(path, out WaveFormat format, out int dataSize);

            if (soundData == IntPtr.Zero)
            {
                return null;
            }

            return new AudioClip(soundData, format, dataSize);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(soundData);
        }

        private static IntPtr LoadSound(string path, out WaveFormat format, out int size)
        {
            if (!File.Exists(path))
            {
                format = new WaveFormat();
                size = 0;
                return IntPtr.Zero;
            }

            size = 0;
            IntPtr unmanagedBuffer;
            using (AudioFileReader reader = new AudioFileReader(path))
            {
                format = new WaveFormat(44100, 16, 2);

                using (MediaFoundationResampler resampler = new MediaFoundationResampler(reader, format))
                {
                    resampler.ResamplerQuality = 60;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;

                        while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memoryStream.Write(buffer, 0, bytesRead);
                        }

                        byte[] fullData = memoryStream.ToArray();
                        unmanagedBuffer = Marshal.AllocHGlobal(fullData.Length);

                        Marshal.Copy(fullData, 0, unmanagedBuffer, fullData.Length);
                        size = fullData.Length;
                    }
                }
            }
            return unmanagedBuffer;
        }
    }
}
