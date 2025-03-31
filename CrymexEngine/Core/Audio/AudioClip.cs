using CrymexEngine.Data;
using CrymexEngine.Debugging;
using NAudio.Lame;
using NAudio.Wave;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CrymexEngine
{
    public class AudioClip : CEDisposable
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

            UsageProfiler.AddMemoryConsumptionValue(dataSize, MemoryUsageType.Audio);
        }

        public static AudioClip? Load(string path)
        {
            IntPtr soundData = LoadSoundFromFile(path, out WaveFormat format, out int dataSize);

            if (soundData == IntPtr.Zero)
            {
                return null;
            }

            return new AudioClip(soundData, format, dataSize);
        }

        public byte[] CompressData()
        {
            byte[] pcmData = new byte[dataSize];
            Marshal.Copy(soundData, pcmData, 0, dataSize);

            using (var pcmStream = new MemoryStream(pcmData))
            using (var reader = new RawSourceWaveStream(pcmStream, new WaveFormat(format.SampleRate, format.BitsPerSample, format.Channels)))
            using (var mp3Stream = new MemoryStream())
            {
                using (var writer = new LameMP3FileWriter(mp3Stream, reader.WaveFormat, LAMEPreset.VBR_90))
                {
                    reader.CopyTo(writer);
                }

                return mp3Stream.ToArray();
            }
        }

        public static AudioClip FromCompressed(byte[] mp3Data)
        {
            WaveFormat format = new WaveFormat(41000, 16, 2);

            using var mp3Stream = new MemoryStream(mp3Data);
            using var reader = new Mp3FileReader(mp3Stream);
            using MediaFoundationResampler resampler = new MediaFoundationResampler(reader, format);
            using MemoryStream pcmStream = new MemoryStream();
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                {
                    pcmStream.Write(buffer, 0, bytesRead);
                }

                byte[] fullData = pcmStream.ToArray();
                IntPtr unmanagedBuffer = Marshal.AllocHGlobal(fullData.Length);

                Marshal.Copy(fullData, 0, unmanagedBuffer, fullData.Length);
                return new AudioClip(unmanagedBuffer, format, fullData.Length);
            }
        }

        protected override void OnDispose()
        {
            Marshal.FreeHGlobal(soundData);
            UsageProfiler.AddMemoryConsumptionValue(-dataSize, MemoryUsageType.Audio);
        }

        private static IntPtr LoadSoundFromFile(string path, out WaveFormat format, out int size)
        {
            size = 0;
            format = new WaveFormat(44100, 16, 2);
            if (!File.Exists(path))
            {
                return IntPtr.Zero;
            }

            IntPtr unmanagedBuffer;
            try
            {
                using AudioFileReader reader = new AudioFileReader(path);
                using MediaFoundationResampler resampler = new MediaFoundationResampler(reader, format);
                using MemoryStream memoryStream = new MemoryStream();

                resampler.ResamplerQuality = 60;

                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }

                byte[] fullData = memoryStream.ToArray();
                size = fullData.Length;

                unmanagedBuffer = Marshal.AllocHGlobal(size);

                Marshal.Copy(fullData, 0, unmanagedBuffer, size);
                return unmanagedBuffer;
            }
            catch
            {
                Debug.LogWarning($"Audio clip at '{path}' couldn't be loaded");
                format = new WaveFormat();
                return IntPtr.Zero;
            }
        }
    }
}
