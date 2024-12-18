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
        public static Settings Instance
        {
            get
            {
                return _instance;
            }
        }

        public static string SettingsText
        {
            get
            {
                return _settingsText;
            }
        }
        public static bool Precompiled
        {
            get
            {
                return _precompiled;
            }
        }

        private static readonly List<SettingOption> settings = new List<SettingOption>();
        private static string _settingsText;
        private static bool _precompiled;

        private static Settings _instance = new Settings();

        public void LoadSettings()
        {
            string rawSettingsText;
            string settingsPath = Debug.runtimeAssetsPath + "RuntimeSettings.rtmAsset";
            if (!File.Exists(settingsPath))
            {
                // Load dynamic settings
                rawSettingsText = File.ReadAllText(Debug.assetsPath + "GlobalSettings.txt");
            }
            else
            {
                _precompiled = true;
                // Load precompiled settings
                rawSettingsText = Encoding.Unicode.GetString(AssetCompiler.DecompileData(File.ReadAllBytes(settingsPath), out _));
            }

            string[] settingsLines = rawSettingsText.Split('\n', StringSplitOptions.TrimEntries);
            for (int i = 0; i < settingsLines.Length; i++)
            {
                string[]? split = FormatSettingLine(settingsLines[i]);
                if (split == null) continue;

                SettingOption? newSetting = CompileSetting(split[0], split[1]);
                if (newSetting == null) continue;
                settings.Add(newSetting);
            }
        }

        public static SettingOption? GetSetting(string name)
        {
            for (int s = 0; s < settings.Count; s++)
            {
                if (settings[s].name == name)
                {
                    return settings[s];
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
            for (int s = 0; s < settings.Count; s++)
            {
                if (settings[s].name == name)
                {
                    if (type == settings[s].type || type == null)
                    {
                        setting = settings[s];
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
    }
}
