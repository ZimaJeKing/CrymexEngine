using CrymexEngine.Data;
using CrymexEngine.Debugging;
using System.Runtime.InteropServices;

namespace CrymexEngine.Audio
{
    public class AudioClip : CEDisposable
    {
        public readonly IntPtr soundData;
        public readonly int dataSize;
        public readonly int sampleRate;
        public readonly int channels;
        public readonly int bitRate;
        public readonly float length;

        public AudioClip(IntPtr soundData, int dataSize, int sampleRate, int channels)
        {
            this.soundData = soundData;
            this.dataSize = dataSize;
            this.sampleRate = sampleRate;
            this.channels = channels;
            length = dataSize / (float)(sampleRate * channels * 2);

            UsageProfiler.AddMemoryConsumptionValue(dataSize, MemoryUsageType.Audio);
        }

        public static AudioClip? Load(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Audio file at '{path}' not found.");
                return null;
            }

            string extension = Path.GetExtension(path).ToLowerInvariant();
            switch (extension)
            {
                case ".ogg": return VorbisLoad(path);
                case ".wav": return WavLoad(path);
                default:
                    Debug.LogError($"Unsupported audio format: '{extension}'");
                    return null;
            }
        }

        private static AudioClip? VorbisLoad(string path)
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    IntPtr soundData = Vorbis.OggToPCM16(fileStream, out int dataSize, out int sampleRate, out int channels);

                    if (soundData == IntPtr.Zero)
                    {
                        return null;
                    }

                    return new AudioClip(soundData, dataSize, sampleRate, channels);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading audio file: " + e.Message);
                return null;
            }
        }

        private static AudioClip? WavLoad(string path)
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    IntPtr soundData = WavDecoder.WavToPCM16(fileStream, out int dataSize, out int sampleRate, out int channels);

                    if (soundData == IntPtr.Zero)
                    {
                        return null;
                    }

                    return new AudioClip(soundData, dataSize, sampleRate, channels);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading audio file: {e.Message} (source: {e.Source})");
                return null;
            }
        }

        public byte[] CompressData()
        {
            short[] buffer = new short[dataSize / sizeof(short)];
            Marshal.Copy(soundData, buffer, 0, dataSize / 2);

            if (channels == 2)
            {
                (short[] leftChannel, short[] rightChannel) = CEResampler.SplitChannels(buffer);

                rightChannel = CEResampler.CreateDifChannel(leftChannel, rightChannel);

                sbyte[] rightSq = CEResampler.DeltaSqueeze(rightChannel);

                using MemoryStream memStream = new MemoryStream();
                using BinaryWriter writer = new BinaryWriter(memStream);

                writer.Write((int)channels);
                writer.Write((int)sampleRate);

                writer.Write((int)leftChannel.Length * sizeof(short));
                writer.Write(CEResampler.ConvertShortToByte(leftChannel));

                writer.Write((int)rightSq.Length);
                writer.Write(SByteToByteArray(rightSq));

                return memStream.ToArray();
            }
            else if (channels == 1)
            {
                using MemoryStream memStream = new MemoryStream();
                using BinaryWriter writer = new BinaryWriter(memStream);

                writer.Write((int)channels);
                writer.Write((int)sampleRate);

                writer.Write((int)buffer.Length);
                writer.Write(CEResampler.ConvertShortToByte(buffer));

                return memStream.ToArray();
            }
            else throw new ArgumentException("Invalid number of channels. Only 1 or 2 channels are supported.");
        }

        public static AudioClip FromCompressed(byte[] data)
        {
            try
            {
                using MemoryStream memStream = new MemoryStream(data);
                using BinaryReader reader = new BinaryReader(memStream);

                int channels = reader.ReadInt32();
                int sampleRate = reader.ReadInt32();

                byte[]? leftBytes = null, rightSq = null;

                leftBytes = reader.ReadBytes(reader.ReadInt32());
                short[] leftChannel = new short[leftBytes.Length / sizeof(short)];
                Buffer.BlockCopy(leftBytes, 0, leftChannel, 0, leftBytes.Length - (leftBytes.Length % 2));

                if (channels == 2)
                {
                    // 2 channels
                    rightSq = reader.ReadBytes(reader.ReadInt32());
                    short[] rightChannel = CEResampler.RevertDifChannel(leftChannel, CEResampler.DeltaUnsqueeze(ByteToSByteArray(rightSq)));

                    short[] merged = CEResampler.MergeChannels(leftChannel, rightChannel);

                    IntPtr mergedSoundData = Marshal.AllocHGlobal(merged.Length * sizeof(short));
                    Marshal.Copy(merged, 0, mergedSoundData, merged.Length);

                    return new AudioClip(mergedSoundData, merged.Length * sizeof(short), sampleRate, 2);
                }

                // 1 channel
                IntPtr leftSoundData = Marshal.AllocHGlobal(leftChannel.Length * sizeof(short));
                Marshal.Copy(leftChannel, 0, leftSoundData, leftChannel.Length);
                return new AudioClip(leftSoundData, leftChannel.Length * sizeof(short), sampleRate, 1);
            }
            catch (Exception e)
            {
                Debug.LogError("Error decompressing audio data: " + e.Message);
                return null;
            }
        }

        public static byte[] SByteToByteArray(sbyte[] source)
        {
            byte[] result = new byte[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = unchecked((byte)source[i]);
            }
            return result;
        }

        public static sbyte[] ByteToSByteArray(byte[] source)
        {
            sbyte[] result = new sbyte[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                result[i] = unchecked((sbyte)source[i]);
            }
            return result;
        }

        protected override void OnDispose()
        {
            Marshal.FreeHGlobal(soundData);
            UsageProfiler.AddMemoryConsumptionValue(-dataSize, MemoryUsageType.Audio);
        }
    }
}
