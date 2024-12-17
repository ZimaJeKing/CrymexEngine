using OpenTK.Audio.OpenAL;
using OpenTK.Platform.Windows;

namespace CrymexEngine
{
    public class AudioSource
    {
        public readonly AudioClip clip;
        public readonly AudioMixer? mixer;
        public readonly bool looping;
        public readonly bool deleteAfterStop;
        public readonly float lengthWithPitch;
        public readonly bool deleteAfterEnd;

        public bool Playing
        {
            get
            {
                return _playing;
            }
        }

        public float Volume
        {
            set
            {
                if (value < 0) value = 0;
                else if (value > 10) value = 10;

                if (mixer != null)
                {
                    AL.Source(alSource, ALSourcef.Gain, value * mixer.Volume);
                }
                else
                {
                    AL.Source(alSource, ALSourcef.Gain, value);
                }
            }
        }

        public bool Disposed
        {
            get 
            { 
                return _disposed; 
            }
        }

        public bool ShouldBeDeleted
        {
            get
            {
                if (!looping && Time.GameTime - _startTime + _playProgress > lengthWithPitch) return true;

                return false;
            }
        }

        private readonly int alDataBuffer;
        private readonly int alSource;

        private bool _playing;
        private float _playProgress;
        private float _startTime;
        private bool _disposed;

        public AudioSource(AudioClip clip, float volume, bool looping = false, bool playOnStart = true, AudioMixer? mixer = null, bool deleteAfterEnd = true)
        {
            this.clip = clip;
            this.looping = looping;
            this.mixer = mixer;
            this.deleteAfterEnd = deleteAfterEnd;
            _startTime = Time.GameTime;

            // Calculate modified length
            lengthWithPitch = clip.length;
            if (mixer != null)
            {
                lengthWithPitch /= mixer.Pitch;
            }

            // Setup OpenAL buffer and source
            alDataBuffer = AL.GenBuffer();
            alSource = AL.GenSource();

            ALFormat format = Audio.GetSoundFormat(clip.format.Channels, clip.format.BitsPerSample);

            AL.BufferData(alDataBuffer, format, clip.soundData, clip.dataSize, clip.format.SampleRate);

            AL.Source(alSource, ALSourcei.Buffer, alDataBuffer);

            if (mixer != null)
            {
                AL.Source(alSource, ALSourcef.Pitch, mixer.Pitch);
            }

            Volume = volume;

            if (playOnStart) Play();
        }

        public void Stop()
        {
            if (_disposed || _playing) return;

            _playing = false;
            AL.SourceStop(alSource);
        }

        public void Play()
        {
            if (_disposed || _playing) return;

            _playProgress = Time.GameTime - _startTime;
            _startTime = Time.GameTime;
            _playing = true;
            AL.SourcePlay(alSource);
        }

        public void Delete()
        {
            Stop();
            Cleanup();
        }

        private void Cleanup()
        {
            if (_disposed || _playing) return;

            _disposed = true;
            AL.SourceStop(alSource);
            AL.DeleteSource(alSource);
            AL.DeleteBuffer(alDataBuffer);
        }
    }
}
