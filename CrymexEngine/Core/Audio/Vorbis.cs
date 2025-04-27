using NVorbis;
using OpenTK.Audio.OpenAL;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CrymexEngine.Audio
{
    public static class Vorbis
    {
        public static IntPtr OggToPCM16(Stream stream, out int dataSize, out int sampleRate, out int channels)
        {
            if (!stream.CanRead) throw new ArgumentException("Stream must be readable");

            VorbisReader reader = new VorbisReader(stream, true);
            sampleRate = reader.SampleRate;
            channels = reader.Channels;

            // Estimate the size (but ensure we can safely handle the total number of samples)
            dataSize = (int)(reader.TotalTime.TotalSeconds * sampleRate * channels * sizeof(short));

            // Ensure divisibility by 2 and 4
            if (dataSize % 2 == 1) dataSize++;
            if (dataSize % 4 == 2) dataSize += 2;

            // Allocate memory for the PCM data
            IntPtr pcmBuffer = Marshal.AllocHGlobal(dataSize);

            float[] buffer = new float[4096];
            int byteIndex = 0;

            int read;
            while ((read = reader.ReadSamples(buffer, 0, buffer.Length)) > 0)
            {
                // Resample float to short
                short[] resampledBuffer = CEResampler.ResampleFloatToShort(buffer, read);

                if (byteIndex + (read * sizeof(short)) > dataSize)
                {
                    read = (dataSize - byteIndex) / sizeof(short);
                }

                // Copy resampled data to unmanaged memory (PCM buffer)
                Marshal.Copy(resampledBuffer, 0, pcmBuffer + byteIndex, read);

                // Update the sample index
                byteIndex += read * sizeof(short);
            }

            return pcmBuffer;
        }
    }
}
