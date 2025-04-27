namespace CrymexEngine.Audio
{
    public static class CEResampler
    {
        private static readonly int[] ADPCMStepSizeTable = new int[]
        {
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
            19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
            50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
            130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
            337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
            876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
            2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
            5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        static readonly int[] ADPCMIndexTable = new int[]
        {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8
        };

        public static float[] ConvertByteToFloat(byte[] buffer)
        {
            float[] resampledBuffer = new float[buffer.Length / sizeof(float)];

            Buffer.BlockCopy(buffer, 0, resampledBuffer, 0, buffer.Length);

            return resampledBuffer;
        }

        public static short[] ConvertByteToShort(byte[] buffer)
        {
            short[] resampledBuffer = new short[buffer.Length / sizeof(short)];

            Buffer.BlockCopy(buffer, 0, resampledBuffer, 0, buffer.Length);

            return resampledBuffer;
        }

        public static byte[] ConvertShortToByte(short[] buffer)
        {
            byte[] resampledBuffer = new byte[buffer.Length * sizeof(short)];

            Buffer.BlockCopy(buffer, 0, resampledBuffer, 0, buffer.Length * sizeof(short));

            return resampledBuffer;
        }

        public static short[] ResamplePCM32ToPCM16(byte[] buffer)
        {
            short[] resampledBuffer = new short[buffer.Length / sizeof(int)];

            for (int i = 0; i < buffer.Length / sizeof(int); i++)
            {
                resampledBuffer[i] = (short)(BitConverter.ToInt32(buffer, i * 4) / short.MaxValue);
            }

            return resampledBuffer;
        }

        public static short[] ResampleALawToPCM16(byte[] buffer)
        {
            short[] resampledBuffer = new short[buffer.Length];

            for (int i = 0; i < buffer.Length; i++)
            {
                resampledBuffer[i] = ALawToLinear(buffer[i]);
            }

            return resampledBuffer;
        }

        public static short[] ResampleMuLawToPCM16(byte[] buffer)
        {
            short[] resampledBuffer = new short[buffer.Length];

            for (int i = 0; i < buffer.Length; i++)
            {
                resampledBuffer[i] = MuLawToLinear(buffer[i]);
            }

            return resampledBuffer;
        }

        public static short[] ResampleIMAADPCMToPCM16(byte[] buffer, int blockAlign, int samplesPerBlock, int channels)
        {
            List<short> left = new();
            List<short> right = new();

            using var reader = new BinaryReader(new MemoryStream(buffer));

            while (reader.BaseStream.Position + blockAlign <= reader.BaseStream.Length)
            {
                byte[] block = reader.ReadBytes(blockAlign);

                if (channels == 1)
                {
                    DecodeMonoBlock(block, left, samplesPerBlock);
                }
                else if (channels == 2)
                {
                    DecodeStereoBlock(block, left, right, samplesPerBlock);
                }
                else
                {
                    throw new NotSupportedException($"Unsupported channel count: {channels}");
                }
            }

            return channels == 1 ? left.ToArray() : MergeChannels(left.ToArray(), right.ToArray());
        }


        private static void DecodeMonoBlock(byte[] block, List<short> output, int samplesPerBlock)
        {
            using var reader = new BinaryReader(new MemoryStream(block));

            short predictor = reader.ReadInt16();
            byte index = reader.ReadByte();
            reader.ReadByte(); // Reserved

            output.Add(predictor);

            int step = ADPCMStepSizeTable[index];
            int sample = predictor;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                byte compressedByte = reader.ReadByte();
                for (int nibbleShift = 0; nibbleShift <= 4; nibbleShift += 4)
                {
                    int nibble = (compressedByte >> nibbleShift) & 0x0F;

                    sample = DecodeSample(sample, nibble, ref index, ref step);
                    output.Add((short)sample);

                    if (output.Count % samplesPerBlock == 0)
                        return; // Done with this block
                }
            }
        }

        private static void DecodeStereoBlock(byte[] block, List<short> left, List<short> right, int samplesPerBlock)
        {
            using var reader = new BinaryReader(new MemoryStream(block));

            short predictorLeft = reader.ReadInt16();
            byte indexLeft = reader.ReadByte();
            reader.ReadByte(); // Reserved

            short predictorRight = reader.ReadInt16();
            byte indexRight = reader.ReadByte();
            reader.ReadByte(); // Reserved

            left.Add(predictorLeft);
            right.Add(predictorRight);

            int stepLeft = ADPCMStepSizeTable[indexLeft];
            int sampleLeft = predictorLeft;

            int stepRight = ADPCMStepSizeTable[indexRight];
            int sampleRight = predictorRight;

            int samples = 0;
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                byte compressedByte = reader.ReadByte();
                int nibbleLeft = compressedByte & 0x0F;
                int nibbleRight = (compressedByte >> 4) & 0x0F;

                sampleLeft = DecodeSample(sampleLeft, nibbleLeft, ref indexLeft, ref stepLeft);
                left.Add((short)sampleLeft);

                sampleRight = DecodeSample(sampleRight, nibbleRight, ref indexRight, ref stepRight);
                right.Add((short)sampleRight);

                samples += 2;

                if (samples >= samplesPerBlock)
                    break;
            }
        }

        private static int DecodeSample(int sample, int nibble, ref byte index, ref int step)
        {
            int delta = step >> 3;
            if ((nibble & 1) != 0) delta += step >> 2;
            if ((nibble & 2) != 0) delta += step >> 1;
            if ((nibble & 4) != 0) delta += step;

            if ((nibble & 8) != 0)
                sample -= delta;
            else
                sample += delta;

            sample = Math.Clamp(sample, -32768, 32767);

            index = (byte)Math.Clamp(index + ADPCMIndexTable[nibble], 0, 88);
            step = ADPCMStepSizeTable[index];

            return sample;
        }

        public static short[] ResamplePCM8ToPCM16(byte[] buffer)
        {
            short[] resampledBuffer = new short[buffer.Length];

            for (int i = 0; i < buffer.Length; i++)
            {
                resampledBuffer[i] = (short)((sbyte)(buffer[i] - 128) * byte.MaxValue);
            }

            return resampledBuffer;
        }

        public static short[] ResampleFloatToShort(float[] buffer, int length = int.MaxValue)
        {
            short[] resampledBuffer = new short[buffer.Length];

            length = Math.Min(buffer.Length, length);

            for (int i = 0; i < length; i++)
            {
                short sample = (short)(buffer[i] * short.MaxValue);
                resampledBuffer[i] = sample;
            }

            return resampledBuffer;
        }

        public static (short[], short[]) SplitChannels(short[] buffer)
        {
            short[] leftChannel = new short[buffer.Length / 2];
            short[] rightChannel = new short[buffer.Length / 2];

            for (int i = 0; i < buffer.Length / 2; i++)
            {
                leftChannel[i] = buffer[i * 2];
                rightChannel[i] = buffer[(i * 2) + 1];
            }

            return (leftChannel, rightChannel);
        }

        public static sbyte[] DeltaSqueeze(short[] samples)
        {
            List<sbyte> deltas = new List<sbyte>();
            short last = 0;
            foreach (short sample in samples)
            {
                short delta = (short)(sample - last);
                if (delta > sbyte.MaxValue || delta <= sbyte.MinValue)
                {
                    deltas.Add(sbyte.MinValue);
                    byte[] bytes = BitConverter.GetBytes(delta);
                    sbyte first = (sbyte)(bytes[0] - 128);
                    sbyte second = (sbyte)(bytes[1] - 128);
                    deltas.Add(first);
                    deltas.Add(second);
                    continue;
                }

                deltas.Add((sbyte)delta);
                last = sample;
            }
            return deltas.ToArray();
        }

        public static short[] CreateDifChannel(short[] left, short[] right)
        {
            if (left.Length != right.Length)
            {
                Debug.LogWarning($"Left and right channels must have the same length. (L: {left.Length}; R: {right.Length})");
                return null;
            }
            else if (left.Length == 0)
            {
                Debug.LogWarning($"Left and right channels must have a length greater than 0. (L: {left.Length}; R: {right.Length})");
                return null;
            }

            short[] deltas = new short[left.Length];

            for (int i = 0; i < left.Length; i++)
            {
                deltas[i] = (short)(right[i] - left[i]);
            }

            return deltas;
        }

        public static short[] RevertDifChannel(short[] left, short[] dif)
        {
            if (left.Length != dif.Length)
            {
                Debug.LogWarning($"Left and dif channels must have the same length. (L: {left.Length}; D: {dif.Length})");
                return null;
            }
            else if (left.Length == 0)
            {
                Debug.LogWarning($"Left and dif channels must have a length greater than 0. (L: {left.Length}; D: {dif.Length})");
                return null;
            }

            short[] right = new short[left.Length];

            for (int i = 0; i < left.Length; i++)
            {
                right[i] = (short)(dif[i] + left[i]);
            }

            return right;
        }

        public static short[] DeltaUnsqueeze(sbyte[] deltas)
        {
            List<short> samples = new List<short>();
            short last = 0;
            for (int i = 0; i < deltas.Length; i++)
            {
                sbyte delta = deltas[i];
                if (delta == sbyte.MinValue)
                {
                    byte first = (byte)(deltas[i + 1] + 128);
                    byte second = (byte)(deltas[i + 2] + 128);

                    short sample = BitConverter.ToInt16([first, second]);
                    samples.Add(sample);
                    last = sample;

                    i += 2;
                    continue;
                }

                short current = (short)(delta + last);
                samples.Add(current);
                last = current;
            }
            return samples.ToArray();
        }

        public static short[] MergeChannels(short[] left, short[] right)
        {
            int length = left.Length;
            if (left.Length != right.Length)
            {
                length = Math.Min(left.Length, right.Length);
                Debug.LogWarning($"Left and right channels must have the same length. (L: {left.Length}; R: {right.Length})");
            }

            short[] buffer = new short[length * 2];
            for (int i = 0; i < length; i++)
            {
                buffer[i * 2] = left[i];
                buffer[(i * 2) + 1] = right[i];
            }

            return buffer;
        }

        private static short ALawToLinear(byte aLaw)
        {
            aLaw ^= 0x55; // Toggle even bits

            int sign = aLaw & 0x80;
            int exponent = (aLaw & 0x70) >> 4;
            int mantissa = aLaw & 0x0F;
            int sample = mantissa << 4;
            sample += 8;
            if (exponent != 0)
                sample += 0x100;
            if (exponent > 1)
                sample <<= exponent - 1;

            return (short)(sign == 0 ? sample : -sample);
        }

        private static short MuLawToLinear(byte muLaw)
        {
            muLaw = (byte)~muLaw;

            int sign = muLaw & 0x80;
            int exponent = (muLaw & 0x70) >> 4;
            int mantissa = muLaw & 0x0F;
            int sample = ((mantissa << 3) + 0x84) << exponent;
            sample -= 0x84;

            return (short)(sign == 0 ? sample : -sample);
        }
    }
}
