using OpenTK.Audio.OpenAL;

namespace CrymexEngine
{
    public class AudioSource
    {
        public readonly float startTime;
        public readonly AudioClip clip;

        public float Volume
        {
            set
            {
                if (value < 0) value = 0;
                else if (value > 10) value = 10;

                if (mixer != null)
                {
                    AL.Source(source, ALSourcef.Gain, value * mixer.Volume);
                }
                else
                {
                    AL.Source(source, ALSourcef.Gain, value);
                }
            }
        }

        public AudioMixer? mixer;

        private int buffer;
        private int source;

        public AudioSource(AudioClip clip, float volume, AudioMixer? mixer)
        {
            this.clip = clip;
            this.mixer = mixer;
            startTime = Window.gameTime;

            buffer = AL.GenBuffer();
            source = AL.GenSource();

            ALFormat format = Audio.GetSoundFormat(clip.format.Channels, clip.format.BitsPerSample);

            AL.BufferData(buffer, format, clip.soundData, clip.dataSize, clip.format.SampleRate);

            AL.Source(source, ALSourcei.Buffer, buffer);

            if (mixer != null)
            {
                AL.Source(source, ALSourcef.Pitch, mixer.Pitch);
            }

            Volume = volume;

            AL.SourcePlay(source);
        }

        public void Cleanup()
        {
            AL.SourceStop(source);
            AL.DeleteSource(source);
            AL.DeleteBuffer(buffer);
        }
    }
}
