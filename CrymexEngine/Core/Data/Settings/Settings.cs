using CrymexEngine.Data;
using OpenTK.Mathematics;
using System.Text;

namespace CrymexEngine
{
    public class Settings
    {
        /// <summary>
        /// An internal instance
        /// </summary>
        public static Settings Instance => _instance;

        public static string SettingsText => _settingsText;
        public bool Precompiled => _precompiled;

        private static readonly List<SettingOption> settings = new List<SettingOption>();
        private static string _settingsText;
        private static bool _precompiled;
        private static bool _settingsLoaded = false;

        private static Settings _instance = new Settings();

        internal void LoadSettings()
        {
            if (_settingsLoaded) return;

            InitializeDirectories();

            string rawSettingsText;
            string settingsPath = IO.runtimeAssetsPath + "RuntimeSettings.rtmAsset";
            if (!File.Exists(settingsPath))
            {
                settingsPath = IO.assetsPath + "GlobalSettings.txt";

                // Load dynamic settings
                if (!File.Exists(settingsPath))
                {
                    Debug.LogWarning($"No settings file found. Created file at \"{settingsPath}\"");
                    File.Create(settingsPath).Dispose();
                    return;
                }
                else
                {
                    rawSettingsText = File.ReadAllText(settingsPath);
                }
            }
            else
            {
                _precompiled = true;
                // Load precompiled settings
                rawSettingsText = Encoding.Unicode.GetString(AssetCompiler.DecompileData(File.ReadAllBytes(settingsPath), out _));
            }

            // Load settings from text data
            string[] settingsLines = rawSettingsText.Split('\n', StringSplitOptions.TrimEntries);
            for (int i = 0; i < settingsLines.Length; i++)
            {
                string[]? split = FormatSettingLine(settingsLines[i]);
                if (split == null) continue;

                SettingOption? newSetting = CompileSetting(split[0], split[1]);
                if (newSetting == null) continue;
                settings.Add(newSetting);
            }

            _settingsLoaded = true;
        }

        public static SettingOption? GetSetting(string name)
        {
            foreach (SettingOption option in settings)
            {
                if (option.name == name)
                {
                    SettingOption opt = option;
                    return opt;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a setting
        /// </summary>
        /// <returns>If the setting was found</returns>
        public static bool GetSetting(string name, out SettingOption setting, SettingType? type = null)
        {
            foreach (SettingOption option in settings)
            {
                if (option.name == name)
                {
                    if (type == option.type || type == null)
                    {
                        setting = option;
                        return true;
                    }
                }
            }
            setting = new SettingOption("NULL", SettingType.None, false);
            return false;
        }

        private static string[]? FormatSettingLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            line = line.Trim();
            if (line[..2] == "//") return null;
            string[] split = line.Split(':', StringSplitOptions.TrimEntries);
            if (split.Length < 2) return null;
            split[1] = split[1].Trim();

            _settingsText += line + '\n';

            return split;
        }

        private static SettingOption? CompileSetting(string name, string value)
        {
            if (value == null || value.Length < 1) return null;

            value = value.Trim();

            char first = value[0];
            char last = value[^1];

            if (first == '#') // Hex values
            {
                if (!int.TryParse(value.AsSpan(1), System.Globalization.NumberStyles.HexNumber, null, out int hexValue)) return null;
                return new SettingOption(name, SettingType.Hex, hexValue);
            }
            else if (first == '(' && value[^1] == ')') // Vectors
            {
                string[] strings = value[1..^1].Split(',');

                float[] values = new float[strings.Length];

                for (int i = 0; i < strings.Length; i++)
                {
                    if (!float.TryParse(strings[i], System.Globalization.NumberStyles.Float, null, out values[i])) return null;
                }

                switch (values.Length)
                {
                    case 2:
                        {
                            return new SettingOption(name, SettingType.Vector2, new Vector2(values[0], values[1]));
                        }
                    case 3:
                        {
                            return new SettingOption(name, SettingType.Vector3, new Vector3(values[0], values[1], values[2]));
                        }
                    case 4:
                        {
                            return new SettingOption(name, SettingType.Vector4, new Vector4(values[0], values[1], values[2], values[3]));
                        }
                    default:
                        {
                            return null;
                        }
                }
            }
            else if (first == '"' || first == '\'')
            {
                if (last != '"' && last != '\'' || value.Length < 2) return null;

                return new SettingOption(name, SettingType.String, value[1..^1]);
            }
            else if (last == 'f')
            {
                if (!float.TryParse(value.AsSpan(0, value.Length - 1), System.Globalization.NumberStyles.Float, null, out float num)) return null;
                return new SettingOption(name, SettingType.Float, num);
            }
            else if (int.TryParse(value, out int num))
            {
                return new SettingOption(name, SettingType.Int, num);
            }
            else if (value == "On" || value == "True")
            {
                return new SettingOption(name, SettingType.Bool, true);
            }
            else if (value == "Off" || value == "False")
            {
                return new SettingOption(name, SettingType.Bool, false);
            }
            return new SettingOption(name, SettingType.RefString, value);
        }

        private static void InitializeDirectories()
        {
            if (!Directory.Exists(IO.assetsPath)) Directory.CreateDirectory(IO.assetsPath);
            if (!Directory.Exists(IO.runtimeAssetsPath)) Directory.CreateDirectory(IO.runtimeAssetsPath);
            if (!Directory.Exists(IO.logFolderPath)) Directory.CreateDirectory(IO.logFolderPath);
            if (!Directory.Exists(IO.saveFolderPath)) Directory.CreateDirectory(IO.saveFolderPath);
        }
    }
}
