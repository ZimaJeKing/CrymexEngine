using NAudio.Wave;

namespace CrymexEngine
{
    public class AudioClip
    {
        public readonly string name;
        public readonly IntPtr soundData;
        public readonly WaveFormat format;
        public readonly int dataSize;
        public readonly float length;

        public AudioClip(string name, IntPtr soundData, WaveFormat format, int dataSize)
        {
            this.name = name;
            this.soundData = soundData;
            this.format = format;
            this.dataSize = dataSize;
            length = dataSize / (float)(format.SampleRate * format.Channels * (format.BitsPerSample / 8f));
        }
    }
}
