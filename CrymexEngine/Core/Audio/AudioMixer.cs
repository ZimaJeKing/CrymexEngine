using System;

namespace CrymexEngine
{
    public class AudioMixer
    {
        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (value < 0) value = 0;
                else if (value > 1) value = 1;

                _volume = value;
            }
        }
        private float _volume;

        public float Pitch
        {
            get
            {
                return _pitch;
            }
            set
            {
                if (value < 0.5f) value = 0.5f;
                else if (value > 2) value = 2;

                _pitch = value;
            }
        }
        private float _pitch;

        public AudioMixer(float volume, float pitch)
        {
            Volume = volume;
            Pitch = pitch;
        }
    }
}
