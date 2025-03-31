using OpenTK.Audio.OpenAL;

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
                if (!_playing) return false;
                if (Progress >= 1)
                {
                    _playing = false;
                    return false;
                }
                return true;
            }
            set
            {
                if (value) Play();
                else Stop();
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
                    AL.Source(_alSource, ALSourcef.Gain, value * mixer.Volume);
                }
                else
                {
                    AL.Source(_alSource, ALSourcef.Gain, value);
                }
            }
        }

        /// <summary>
        /// A value between 0 and 1 representing the play progress
        /// </summary>
        public float Progress
        {
            get
            {
                if (!_playing)
                {
                    return _playProgress / lengthWithPitch;
                }

                float val = (Time.GameTime - _startTime + _playProgress) / lengthWithPitch;

                if (looping)
                {
                    val %= 1;
                }

                return val;
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
                if (Progress > 1 && deleteAfterEnd)
                {
                    return true;
                }

                return false;
            }
        }

        private readonly int alDataBuffer;
        private readonly int _alSource;

        private bool _playing = false;
        private float _playProgress = 0;
        private float _startTime = -1;
        private bool _disposed = false;

        public AudioSource(AudioClip clip, float volume, bool looping = false, bool playOnStart = true, AudioMixer? mixer = null, bool deleteAfterEnd = true)
        {
            if (!Audio.Initialized)
            {
                Debug.LogError("Audio module not initialized. Cannot create an audio source instance");
                return;
            }
            if (clip.Disposed)
            {
                _disposed = true;
                return;
            }

            this.clip = clip;
            this.mixer = mixer;
            _startTime = Time.GameTime;
            if (looping)
            {
                this.looping = true;
                this.deleteAfterEnd = false;
            }
            else
            {
                this.looping = false;
                this.deleteAfterEnd = deleteAfterEnd;
            }

            // Calculate modified length
            lengthWithPitch = clip.length;
            if (mixer != null)
            {
                lengthWithPitch /= mixer.Pitch;
            }

            // Setup OpenAL buffer and source
            alDataBuffer = AL.GenBuffer();
            _alSource = AL.GenSource();

            ALFormat format = Audio.GetSoundFormat(clip.format.Channels, clip.format.BitsPerSample);

            AL.BufferData(alDataBuffer, format, clip.soundData, clip.dataSize, clip.format.SampleRate);

            AL.Source(_alSource, ALSourcei.Buffer, alDataBuffer);

            if (mixer != null)
            {
                AL.Source(_alSource, ALSourcef.Pitch, mixer.Pitch);
            }

            if (looping) AL.Source(_alSource, ALSourceb.Looping, true);

            Volume = volume;

            if (playOnStart) Play();
        }

        public void Stop()
        {
            if (_disposed || !_playing) return;

            _playing = false;
            _playProgress = 0;
            _startTime = 0;

            AL.SourceStop(_alSource);
        }

        public void Play()
        {
            if (_disposed || _playing) return;
            Stop();

            _playing = true;
            _startTime = Time.GameTime;
            AL.SourcePlay(_alSource);
        }

        public void Pause()
        {
            if (_disposed) return;

            if (_playing)
            {
                _playing = false;
                _playProgress += Time.GameTime - _startTime;
                AL.SourcePause(_alSource);
            }
            else
            {
                _playing = true;
                _startTime = Time.GameTime;
                AL.SourcePlay(_alSource);
            }

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
            AL.SourceStop(_alSource);
            AL.DeleteSource(_alSource);
            AL.DeleteBuffer(alDataBuffer);
        }
    }
}
