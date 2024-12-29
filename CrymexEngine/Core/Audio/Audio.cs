using OpenTK.Audio.OpenAL;

namespace CrymexEngine
{
    public class Audio
    {
        /// <summary>
        /// An internal instance
        /// </summary>
        public static Audio Instance
        {
            get 
            { 
                return _instance; 
            }
        }

        public static bool Initialized
        {
            get
            {
                return _initialized;
            }
        }

        private static readonly List<AudioSource> _sources = new List<AudioSource>();
        private static bool _initialized = false;
        private static ALDevice _alDevice;
        private static ALContext _alContext;

        private static Audio _instance = new Audio();

        public void InitializeContext()
        {
            Cleanup();

            _alDevice = ALC.OpenDevice(null);
            if (_alDevice == IntPtr.Zero)
            {
                Debug.LogError("Couldn't initialize audio device");
                return;
            }

            _alContext = ALC.CreateContext(_alDevice, (int[]?)null);
            ALC.MakeContextCurrent(_alContext);

            _initialized = true;
        }

        public void RemoveInactiveSources()
        {
            if (!_initialized) return;

            foreach (AudioSource source in _sources)
            {
                if (source.ShouldBeDeleted)
                {
                    source.Delete();
                }
            }
        }

        public static AudioSource Play(AudioClip clip, float volume, bool looping = false, AudioMixer? mixer = null, bool deleteAfterEnd = true)
        {
            if (!_initialized || clip == null) return null;

            AudioSource source = new AudioSource(clip, volume, looping, true, mixer);

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

        public void Cleanup()
        {
            if (!_initialized) return;
            ALC.DestroyContext(_alContext);
            ALC.CloseDevice(_alDevice);
        }
    }
}
