using OpenTK.Audio.OpenAL;

namespace CrymexEngine
{
    public static class Audio
    {
        public static bool Initialized => _initialized;
        public static string? PlaybackDevice => _playbackDevice;
        public static bool OpenALPresent => _openALPresent;

        private static readonly List<AudioSource> _sources = new List<AudioSource>();
        private static bool _initialized = false;
        private static ALDevice _alDevice = ALDevice.Null;
        private static ALContext _alContext;
        private static bool _openALPresent;
        private static string? _playbackDevice;

        internal static bool InitializeContext()
        {
            if (_initialized) return false;
            string? defaultDevice = GetDefaultDevice();

            try
            {
                _alDevice = ALC.OpenDevice(defaultDevice);
                _playbackDevice = ALC.GetString(_alDevice, AlcGetString.AllDevicesSpecifier);
                _openALPresent = true;
            }
            catch (DllNotFoundException)
            {
                Debug.LogError("OpenAL dll not found. Audio module inactive");
                return false;
            }
            if (_alDevice == IntPtr.Zero)
            {
                Debug.LogError("Couldn't initialize audio device");
                return false;
            }

            _alContext = ALC.CreateContext(_alDevice, (int[]?)null);
            ALC.MakeContextCurrent(_alContext);

            _initialized = true;

            Debug.LogLocalInfo("Audio Handler", $"Audio context initialized with device '{_playbackDevice}'");
            return true;
        }

        private static string? GetDefaultDevice()
        {
            string? defaultDevice = null;
            if (Settings.GlobalSettings.GetSetting("DefaultAudioDevice", out SettingOption audioDeviceOption, SettingType.GeneralString))
            {
                string settingValue = audioDeviceOption.GetValue<string>();
                if (settingValue.ToLower() != "null")
                {
                    defaultDevice = settingValue;
                }
            }

            return defaultDevice;
        }

        public static void OverrideContext(string? deviceName = null)
        {
            if (_alDevice != ALDevice.Null) Cleanup();

            try
            {
                _alDevice = ALC.OpenDevice(deviceName);
                _playbackDevice = ALC.GetString(_alDevice, AlcGetString.DeviceSpecifier);
            }
            catch (DllNotFoundException)
            {
                Debug.LogError("OpenAL dll not found. Audio module inactive");
                return;
            }

            if (_alDevice == IntPtr.Zero)
            {
                Debug.LogError("Couldn't initialize audio device");
                return;
            }

            _alContext = ALC.CreateContext(_alDevice, (int[]?)null);
            ALC.MakeContextCurrent(_alContext);

            Debug.LogLocalInfo("Audio Handler", $"Audio context override for device '{_playbackDevice}'");
        }

        public static string[] GetAvailableDevices()
        {
            if (!_initialized) return new string[0];

            return ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier).ToArray();
        }

        internal static void RemoveInactiveSources()
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
            if (!_initialized || clip == null || clip.Disposed) return null;
            AudioSource source = new AudioSource(clip, volume, looping, true, mixer);

           _sources.Add(source);

            return source;
        }

        internal static ALFormat GetSoundFormat(int channels, int bits)
        {
            if (channels == 1 && bits == 8) return ALFormat.Mono8;
            if (channels == 1 && bits == 16) return ALFormat.Mono16;
            if (channels == 2 && bits == 8) return ALFormat.Stereo16;
            if (channels == 2 && bits == 16) return ALFormat.Stereo16;
            Debug.LogError($"Audio: Unsuported sound format (channels: {channels}, bitRate: {bits})");
            return ALFormat.Mono8;
        }

        internal static void Cleanup()
        {
            if (!_initialized || !_openALPresent) return;
            ALC.DestroyContext(_alContext);
            ALC.CloseDevice(_alDevice);
        }
    }
}
