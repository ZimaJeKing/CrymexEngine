using CrymexEngine.Data;
using OpenTK.Mathematics;
using System.Text;

namespace CrymexEngine
{
    public class Settings
    {
        public static readonly Settings GlobalSettings = new Settings();

        public string SettingsText => _settingsText;
        public bool Precompiled => _precompiled;

        private readonly List<SettingOption> settings = new();
        private string _settingsText = string.Empty;
        private bool _precompiled = false;

        public Settings() 
        { 

        }

        /// <summary>
        /// Loads settings data from a file and adds them to the collection
        /// </summary>
        /// <returns></returns>
        public bool LoadFile(string dynamicPath)
        {
            if (!Path.IsPathFullyQualified(dynamicPath))
            {
                Debug.LogError($"Settings path is in wrong format '{dynamicPath}'");
                return false;
            }

            string settingsFileName = Path.GetFileNameWithoutExtension(dynamicPath);
            string rawSettingsText;
            string precompiledPath = Directories.runtimeAssetsPath + settingsFileName + ".settingsFile";
            if (!File.Exists(precompiledPath))
            {
                // Load dynamic settings
                if (!File.Exists(dynamicPath))
                {
                    Debug.LogWarning($"No settings file found. Created file at '{dynamicPath}'");
                    File.Create(dynamicPath).Dispose();
                    return false;
                }
                else
                {
                    rawSettingsText = File.ReadAllText(dynamicPath);
                }
            }
            else
            {
                _precompiled = true;
                // Load precompiled settings
                rawSettingsText = Encoding.Unicode.GetString(AssetCompiler.DecompileData(File.ReadAllBytes(precompiledPath), out _));
            }

            // Load settings from text data
            string[] settingsLines = rawSettingsText.Split('\n', StringSplitOptions.TrimEntries);
            for (int i = 0; i < settingsLines.Length; i++)
            {
                string[]? split = FormatSettingLine(settingsLines[i]);
                if (split == null || split.Length < 2) continue;

                SettingOption? newSetting = CompileSetting(split[0], split[1]);
                if (newSetting == null) continue;

                settings.Add(newSetting);
                _settingsText += settingsLines[i];
            }

            return true;
        }

        public SettingOption? GetSetting(string name)
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
        public bool GetSetting(string name, out SettingOption setting, SettingType? type = null)
        {
            foreach (SettingOption option in settings)
            {
                if (option.name == name)
                {
                    if (type == option.type)
                    {
                        setting = option;
                        return true;
                    }
                }
            }
            setting = new SettingOption("NULL", SettingType.None, false);
            return false;
        }

        private string[]? FormatSettingLine(string line)
        {
            if (string.IsNullOrEmpty(line) || line.Length < 2) return null;
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
