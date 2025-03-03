using CrymexEngine.Data;
using CrymexEngine.Debugging;
using OpenTK.Graphics.ES20;
using OpenTK.Mathematics;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace CrymexEngine
{
    public class Settings
    {
        public static readonly Settings GlobalSettings = new Settings();

        public readonly List<SettingOption> options = new();
        public string SettingsText => _settingsText;
        public bool Precompiled => _precompiled;

        private string _settingsText = string.Empty;
        private bool _precompiled = false;

        private static readonly string _defaultGlobalSettingsText = "LogToConsole:True\nVSync:True\nLogFPS:True";

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
            string precompiledPath = Directories.RuntimeAssetsPath + settingsFileName + ".settingsFile";
            if (!File.Exists(precompiledPath))
            {
                // Load dynamic settings
                if (!File.Exists(dynamicPath))
                {
                    Debug.LogWarning($"No settings file found. Created file at '{dynamicPath}'");
                    try
                    {
                        // Create an empty settings file
                        File.Create(dynamicPath).Dispose();

                        // If creating GlobalSettings, write some default settings into the file
                        if (Path.GetFileNameWithoutExtension(dynamicPath) == "GlobalSettings")
                        {
                            File.WriteAllText(dynamicPath, _defaultGlobalSettingsText);
                            rawSettingsText = _defaultGlobalSettingsText;
                        }
                        else return false;
                    }
                    catch (Exception ex) 
                    { 
                        Debug.LogError($"[{Path.GetFileName(dynamicPath)}] {ex.Message}");
                        return false;
                    }
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

            // Add memory consumption value (2 * text length for unicode)
            UsageProfiler.AddMemoryConsumptionValue(rawSettingsText.Length * 2, MemoryUsageType.Other);

            // Load settings from text data
            string[] settingsLines = rawSettingsText.Split('\n', StringSplitOptions.TrimEntries);
            for (int i = 0; i < settingsLines.Length; i++)
            {
                string[]? split = FormatSettingLine(settingsLines[i]);
                if (split == null || split.Length < 2) continue;

                SettingOption? newSetting = DecompileSetting(split[0], split[1]);
                if (newSetting == null) continue;

                options.Add(newSetting);
                _settingsText += settingsLines[i] + '\n';
            }

            return true;
        }

        public SettingOption? GetSetting(string name)
        {
            foreach (SettingOption option in options)
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
        /// <returns>True, if the setting was found</returns>
        public bool GetSetting(string name, out SettingOption setting, SettingType? type = null)
        {
            foreach (SettingOption option in options)
            {
                if (option.name == name && option.type != SettingType.None)
                {
                    if (type == SettingType.GeneralNumber && (option.type == SettingType.Float || option.type == SettingType.Int || option.type == SettingType.Hex))
                    {
                        setting = option;
                        return true;
                    }
                    if (type == SettingType.GeneralString && (option.type == SettingType.String || option.type == SettingType.RefString))
                    {
                        setting = option;
                        return true;
                    }
                    if (type == option.type)
                    {
                        setting = option;
                        return true;
                    }
                }
            }
            setting = SettingOption.None;
            return false;
        }

        private static string[]? FormatSettingLine(string line)
        {
            if (string.IsNullOrEmpty(line) || line.Length < 2) return null;
            line = line.Trim();
            if (line[..2] == "//") return null;
            string[] split = line.Split(':', StringSplitOptions.TrimEntries);
            if (split.Length < 2) return null;
            split[1] = split[1].Trim();

            return split;
        }

        private static SettingOption? DecompileSetting(string name, string value)
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
