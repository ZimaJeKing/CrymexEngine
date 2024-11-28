using OpenTK.Mathematics;
using System;

namespace CrymexEngine
{
    public static class Settings
    {
        private static List<SettingOption> settings = new List<SettingOption>();

        public static void LoadSettings()
        {
            string globalSettingsPath = Debug.assetsPath + "GlobalSettings.txt";
            if (!File.Exists(globalSettingsPath)) File.Create(globalSettingsPath).Close();

            string[] settingsLines = File.ReadAllLines(globalSettingsPath);

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

        public static bool GetSetting(string name, out SettingOption setting)
        {
            for (int s = 0; s < settings.Count; s++)
            {
                if (settings[s].name == name)
                {
                    setting = settings[s];
                    return true;
                }
            }
            setting = null;
            return false;
        }

        public static void SetSetting(string name, SettingType type, object value)
        {
            string globalSettingsPath = Debug.assetsPath + "GlobalSettings.txt";
            if (!File.Exists(globalSettingsPath)) File.Create(globalSettingsPath).Close();

            string[] settingsLines = File.ReadAllLines(globalSettingsPath);

            for (int i = 0; i < settingsLines.Length; i++)
            {
                string[]? split = FormatSettingLine(settingsLines[i]);
                if (split == null) continue;

                SettingOption? newSetting = CompileSetting(split[0], split[1]);
                if (newSetting == null) continue;

                if (newSetting.name == name && newSetting.type == type)
                {
                    settingsLines[i] = new SettingOption(name, type, value).ToString();
                    return;
                }
            }

            settingsLines[settingsLines.Length - 1] += "\n\n" + new SettingOption(name, type, value).ToString();
        }

        private static string[]? FormatSettingLine(string line)
        {
            if (line == "") return null;
            line = line.Trim();
            if (line.Substring(0, 2) == "//") return null;
            string[] split = line.Split(':', StringSplitOptions.TrimEntries);
            if (split.Length < 2) return null;
            split[1] = split[1].Trim();

            return split;
        }

        private static SettingOption? CompileSetting(string name, string value)
        {
            if (value == null || value.Length < 1) return null;

            value = value.Trim();

            char first = value[0];
            char last = value[value.Length - 1];

            if (first == '#') // Hex values
            {
                if (!int.TryParse(value.Substring(1), System.Globalization.NumberStyles.HexNumber, null, out int hexValue)) return null;
                return new SettingOption(name, SettingType.Hex, hexValue);
            }
            else if (first == '(' && value[value.Length - 1] == ')') // Vectors
            {
                string[] strings = value.Substring(1, value.Length - 2).Split(',');

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

                return new SettingOption(name, SettingType.String, value.Substring(1, value.Length - 2));
            }
            else if (last == 'f')
            {
                if (!float.TryParse(value.Substring(0, value.Length - 1), System.Globalization.NumberStyles.Float, null, out float num)) return null;
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
