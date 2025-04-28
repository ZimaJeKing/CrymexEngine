using System.Runtime.InteropServices;

namespace CrymexEngine.Audio
{
    public static class WavDecoder
    {
        private static readonly WavFormat[] decodeableFormats = [ WavFormat.PCM, WavFormat.IEEE_Float, WavFormat.ALaw, WavFormat.MuLaw ];
        private static readonly string[] dataChunkIDs = ["data", "wave", "LIST", "bext", "sampl", "cue ", ];

        /// <returns>IntPtr.Zero if format cannot be decoded</returns>
        public static IntPtr WavToPCM16(Stream stream, out int dataSize, out int sampleRate, out int channels)
        {
            WavFormat audioFormat = WavFormat.Unknown;
            using BinaryReader reader = new BinaryReader(stream);

            dataSize = 0;
            sampleRate = 0;
            channels = 0;

            // Read the RIFF header
            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF")
            {
                Debug.LogError("Invalid RIFF header");
                return IntPtr.Zero;
            }

            // Read WAVE format
            uint fileSize = reader.ReadUInt32();
            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE")
            {
                Debug.LogError("Invalid WAVE header");
                return IntPtr.Zero;
            }

            byte[]? dataBuffer = null;
            short[]? pcmBuffer = null;
            ushort bitsPerSample = 0;
            ushort samplesPerBlock = 0;
            ushort blockAlign = 0;

            while (stream.Position < stream.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                uint chunkSize = reader.ReadUInt32();
                long chunkStart = stream.Position;

                if (chunkId == "fmt ")
                {
                    audioFormat = (WavFormat)reader.ReadUInt16();
                    channels = (int)reader.ReadUInt16();
                    sampleRate = (int)reader.ReadUInt32();
                    reader.ReadUInt32(); // byte rate (skip)
                    blockAlign = reader.ReadUInt16();
                    bitsPerSample = reader.ReadUInt16();
                    ushort extraFormatSize = reader.ReadUInt16();

                    if (audioFormat == WavFormat.IMA_ADPCM && extraFormatSize >= 2)
                    {
                        samplesPerBlock = reader.ReadUInt16();
                    }

                    // If detected format is not supported, throw an error
                    if (!decodeableFormats.Contains(audioFormat))
                    {
                        Debug.LogWarning($"Unsupported Wav format '{audioFormat}'");
                        return IntPtr.Zero;
                    }

                    // Skip the rest of the fmt chunk
                    stream.Seek(chunkStart + chunkSize, SeekOrigin.Begin);
                }
                else if (dataChunkIDs.Contains(chunkId) && (dataBuffer == null || dataBuffer.Length < 16))
                {
                    // Read the PCM data
                    dataBuffer = new byte[chunkSize];
                    reader.Read(dataBuffer, 0, (int)chunkSize);
                }
                else
                {
                    // Skip unknown chunks
                    stream.Seek(chunkStart + chunkSize, SeekOrigin.Begin);
                }
            }

            if (dataBuffer == null)
            {
                Debug.LogError("No PCM data found");
                return IntPtr.Zero;
            }

            pcmBuffer = ConvertToPCM16(audioFormat, dataBuffer, bitsPerSample, samplesPerBlock, blockAlign, channels);

            if (pcmBuffer == null)
            {
                Debug.LogError($"Error converting wav data to short array (format: {audioFormat})");
                return IntPtr.Zero;
            }

            dataSize = pcmBuffer.Length * sizeof(short);
            IntPtr pcmBufferPtr = Marshal.AllocHGlobal(dataSize);
            Marshal.Copy(pcmBuffer, 0, pcmBufferPtr, pcmBuffer.Length);
            return pcmBufferPtr;
        }

        public static byte[] CreateWavHeader(int sampleRate, int channels, int dataSize, WavFormat format = WavFormat.PCM)
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write("RIFF".ToCharArray());
            writer.Write((uint)(36 + dataSize));
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write((uint)16); // fmt chunk size
            writer.Write((ushort)format);
            writer.Write((ushort)channels);
            writer.Write((uint)sampleRate);
            writer.Write((uint)(sampleRate * channels * 2)); // byte rate
            writer.Write((ushort)(channels * 2)); // block align

            writer.Write((ushort)8); // bits per sample

            writer.Write("data".ToCharArray());
            writer.Write((uint)dataSize);

            return stream.ToArray();
        }

        private static short[]? ConvertToPCM16(WavFormat audioFormat, byte[] dataBuffer, ushort bitsPerSample, ushort samplesPerBlock, ushort blockAlign, int channels)
        {
            short[]? pcmBuffer = null;

            if (audioFormat == WavFormat.IEEE_Float) pcmBuffer = CEResampler.ResampleFloatToShort(CEResampler.ConvertByteToFloat(dataBuffer));
            else if (audioFormat == WavFormat.PCM)
            {
                if (bitsPerSample == 16) pcmBuffer = CEResampler.ConvertByteToShort(dataBuffer);
                else if (bitsPerSample == 32) pcmBuffer = CEResampler.ResamplePCM32ToPCM16(dataBuffer);
                else if (bitsPerSample == 8) pcmBuffer = CEResampler.ResamplePCM8ToPCM16(dataBuffer);
                else
                {
                    throw new Exception("Unsupported bits per sample format: " + bitsPerSample);
                }
            }
            else if (audioFormat == WavFormat.ALaw) pcmBuffer = CEResampler.ResampleALawToPCM16(dataBuffer);
            else if (audioFormat == WavFormat.MuLaw) pcmBuffer = CEResampler.ResampleMuLawToPCM16(dataBuffer);
            else if (audioFormat == WavFormat.IMA_ADPCM)
            {
                pcmBuffer = CEResampler.ResampleIMAADPCMToPCM16(dataBuffer, blockAlign, samplesPerBlock, channels);
            }
            return pcmBuffer;
        }
    }

    public enum WavFormat
    {
        Unknown = 0,
        PCM = 1,
        IEEE_Float = 3,
        IMA_ADPCM = 17,
        MS_ADPCM = 2,
        ALaw = 6,
        MuLaw = 7,
        Mp21 = 80,
        Mp3 = 85,
        WMA = 353,
        EXTENSIBLE = 65534,
    }
}
