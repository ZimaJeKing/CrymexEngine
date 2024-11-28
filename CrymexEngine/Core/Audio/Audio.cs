using NAudio.Mixer;
using NAudio.Wave;
using OpenTK.Audio.OpenAL;

namespace CrymexEngine
{
    public static class Audio
    {
        private static bool initialized = false;
        private static List<AudioSource> sources = new List<AudioSource>();
        private static ALDevice device;
        private static ALContext context;

        public static void Init()
        {
            device = ALC.OpenDevice(null);
            if (device == IntPtr.Zero)
            {
                Debug.Log("Couldn't initialize audio device", ConsoleColor.DarkRed);
                return;
            }

            context = ALC.CreateContext(device, (int[]?)null);
            ALC.MakeContextCurrent(context);

            initialized = true;
        }

        public static void Update()
        {
            for (int i = 0; i < sources.Count; i++)
            {
                AudioSource source = sources[i];

                float lengthWithPitch = source.clip.length;
                if (source.mixer != null)
                {
                    lengthWithPitch /= source.mixer.Pitch;
                }
                if (Window.gameTime - source.startTime > lengthWithPitch)
                {
                    source.Cleanup();
                    sources.Remove(source);
                }
            }
        }

        public static AudioSource Play(AudioClip clip, float volume = 1, AudioMixer? mixer = null)
        {
            if (!initialized || clip == null) return null;

            AudioSource source = new AudioSource(clip, volume, mixer);

            sources.Add(source);

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
            ALC.DestroyContext(context);
            ALC.CloseDevice(device);
        }
    }
}
