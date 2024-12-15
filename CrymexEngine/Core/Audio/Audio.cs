using NAudio.Mixer;
using NAudio.Wave;
using OpenTK.Audio.OpenAL;

namespace CrymexEngine
{
    public static class Audio
    {
        private static bool _initialized = false;
        private static List<AudioSource> _sources = new List<AudioSource>();
        private static ALDevice _alDevice;
        private static ALContext _alContext;

        public static void Init()
        {
            _alDevice = ALC.OpenDevice(null);
            if (_alDevice == IntPtr.Zero)
            {
                Debug.Log("Couldn't initialize audio device", ConsoleColor.DarkRed);
                return;
            }

            _alContext = ALC.CreateContext(_alDevice, (int[]?)null);
            ALC.MakeContextCurrent(_alContext);

            _initialized = true;
        }

        public static void Update()
        {
            if (!_initialized) return;

            for (int i = 0; i < _sources.Count; i++)
            {
                AudioSource source = _sources[i];

                float lengthWithPitch = source.clip.length;
                if (source.mixer != null)
                {
                    lengthWithPitch /= source.mixer.Pitch;
                }
                if (Time.GameTime - source.startTime > lengthWithPitch)
                {
                    source.Cleanup();
                    _sources.Remove(source);
                }
            }
        }

        public static AudioSource Play(AudioClip clip, float volume = 1, AudioMixer? mixer = null)
        {
            if (!_initialized || clip == null) return null;

            AudioSource source = new AudioSource(clip, volume, mixer);

            _sources.Add(source);

            return source;
        }

        public static ALFormat GetSoundFormat(int channels, int bits)
        {
            if (channels == 1 && bits == 8) return ALFormat.Mono8;
            if (channels == 1 && bits == 16) return ALFormat.Mono16;
            if (channels == 2 && bits == 8) return ALFormat.Stereo16;
            if (channels == 2 && bits == 16) return ALFormat.Stereo16;
            Debug.LogError($"Audio: Unsuported sound format (channels: {channels}, bitRate: {bits})");
            return ALFormat.Mono8;
        }

        public static void Cleanup()
        {
            if (!_initialized) return;
            ALC.DestroyContext(_alContext);
            ALC.CloseDevice(_alDevice);
        }
    }
}
