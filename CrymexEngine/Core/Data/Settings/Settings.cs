using CrymexEngine.Data;
using CrymexEngine.Debugging;
using CrymexEngine.Utils;
using NAudio.Wave;
using OpenTK.Mathematics;
using System.Text;

namespace CrymexEngine
{
    public class Settings
    {
        public static readonly Settings GlobalSettings = new Settings();

        public readonly bool recompile;

        /// <summary>
        /// Generates a new settings text representation on read
        /// </summary>
        public string AsText => GenerateSettingsText();

        private readonly Dictionary<string, SettingOption> options = new();

        private static readonly string _defaultGlobalSettingsText = "LogToConsole:True\nVSync:True\nLogFPS:True";

        public Settings(bool recompile = true)
        {
            this.recompile = recompile;
        }

        /// <summary>
        /// Loads settings data from a file and adds them to the collection
        /// </summary>
        /// <returns></returns>
        public bool LoadFile(string path)
        {
            if (!Path.IsPathFullyQualified(path))
            {
                Debug.LogError($"Settings path is in wrong format '{path}'");
                return false;
            }

            string rawSettingsText;
            // Load dynamic settings
            if (!File.Exists(path))
            {
                Debug.LogWarning($"No settings file found for '{DataUtil.GetCENameFromPath(path)}'. Created file at '{path}'");
                try
                {
                    // Create an empty settings file
                    File.Create(path).Dispose();

                    // If creating global settings, write some default settings into the file
                    string fileName = Path.GetFileName(path);
                    if (fileName == "CEConfig.cfg")
                    {
                        File.WriteAllText(path, _defaultGlobalSettingsText);
                        rawSettingsText = _defaultGlobalSettingsText;
                    }
                    else if (fileName == "AssetCompilation.cfg")
                    {
                        string assetText = $"TextureCompressionLevel: {Assets.TextureCompressionLevel}";
                        File.WriteAllText(path, assetText);
                        rawSettingsText = assetText;
                    }
                    else return false;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Couldn't load settings file at '{Path.GetFileName(path)}' ({ex.Message})");
                    return false;
                }
            }
            else
            {
                rawSettingsText = File.ReadAllText(path);
            }

            // Load settings from text data
            LoadText(rawSettingsText);

            return true;
        }

        public void LoadText(string text)
        {
            string[] settingsLines = text.Split('\n', StringSplitOptions.TrimEntries);
            for (int i = 0; i < settingsLines.Length; i++)
            {
                (string, string)? formated = FormatSettingLine(settingsLines[i]);
                if (formated == null) continue;
                string name = formated.Value.Item1;
                string value = formated.Value.Item2;

                SettingOption? newSetting;
                if (options.ContainsKey(formated.Value.Item1))
                {
                    options.Remove(name);
                    newSetting = DecompileSetting(name, value);

                    if (newSetting == null) continue;
                    options.Add(name, newSetting);
                    continue;
                }

                newSetting = DecompileSetting(name, value);
                if (newSetting == null) continue;

                options.Add(newSetting.name, newSetting);
            }
        }

        public SettingOption? GetSetting(string name)
        {
            options.TryGetValue(name, out SettingOption? option);
            return option;
        }

        public void SetSetting(string name, object value, SettingType type)
        {
            options.Remove(name);
            options.Add(name, new SettingOption(name, type, value));
        }

        /// <summary>
        /// Gets a setting
        /// </summary>
        /// <returns>True, if the option was found</returns>
        public bool GetSetting(string name, out SettingOption option, SettingType? type = null)
        {
            if (options.TryGetValue(name, out option) && option != null)
            {
                if (option.type != SettingType.None)
                {
                    if (type == SettingType.GeneralNumber && (option.type == SettingType.Float || option.type == SettingType.Int || option.type == SettingType.Hex))
                    {
                        return true;
                    }
                    if (type == SettingType.GeneralString && (option.type == SettingType.String || option.type == SettingType.RefString))
                    {
                        return true;
                    }
                    if (type == option.type)
                    {
                        return true;
                    }
                }
            }
            option = SettingOption.None;
            return false;
        }

        /// <summary>
        /// Recompiles all settings with the made changes
        /// </summary>
        public static void RecompileAllSettings()
        {
            if (!Assets.RunningPrecompiled)
            {
                Debug.LogWarning("Cannot recompile settings while running on dynamic assets");
                return;
            }

            KeyValuePair<string, SettingAsset>[] settings = Assets.GetAllSettingAssets();
            string settingsPath = Directories.RuntimeAssetsPath + "RuntimeSettings.rtmAsset";
            using (FileStream settingsFileStream = File.Create(settingsPath))
            {
                foreach (KeyValuePair<string, SettingAsset> pair in settings.ToArray())
                {
                    if (!pair.Value.settings.recompile) continue;

                    settingsFileStream.Write(AssetCompiler.CompileData(pair.Value.name, Encoding.Unicode.GetBytes(DataUtil.XorString(pair.Value.settings.AsText))));
                }
            }

            // Compile global settings
            RecompileGlobalSettings();
        }

        public static void RecompileGlobalSettings()
        {
            if (!Assets.RunningPrecompiled)
            {
                Debug.LogWarning("Cannot recompile settings while running on dynamic assets");
                return;
            }

            byte[] globalSettingsData = AssetCompiler.CompileData("CEConfig", Encoding.Unicode.GetBytes(DataUtil.XorString(GlobalSettings.AsText)));
            File.WriteAllBytes(Directories.RuntimeAssetsPath + "CEConfig.rtmAsset", globalSettingsData);
        }

        private static (string, string)? FormatSettingLine(string line)
        {
            if (string.IsNullOrEmpty(line) || line.Length < 2) return null;

            line = line.Trim();
            if (line[..2] == "//") return null;

            int splitIndex = line.IndexOf(':');

            DataUtil.RemoveSpecialCharacters(line.Substring(0, splitIndex).Trim(), out string name);
            string value = line.Substring(splitIndex + 1).Trim();

            if (name.Length == 0 ||  value.Length == 0) return null;

            return (name, value);
        }

        private string GenerateSettingsText()
        {
            string text = string.Empty;
            KeyValuePair<string, SettingOption>[] optionArr = options.ToArray();
            for (int i = 0; i < optionArr.Length; i++)
            {
                text += optionArr[i].Value.ToString() + "\r\n";
            }
            return text;
        }

        private static SettingOption? DecompileSetting(string name, string value)
        {
            if (value == null || value.Length < 1 || value.ToLower() == "null") return null;

            value = value.Trim();

            char first = value[0];
            char last = value[^1];

            if (first == '#' && int.TryParse(value.AsSpan(1), System.Globalization.NumberStyles.HexNumber, null, out int hexValue)) // Hex value
            {
                return new SettingOption(name, SettingType.Hex, hexValue);
            }
            else if (first == '(' && value[^1] == ')') // Vector
            {
                return ParseVectorSetting(name, value);
            }
            else if (first == '"' || first == '\'') // String
            {
                if (last != '"' && last != '\'' || value.Length < 2) return null;

                return new SettingOption(name, SettingType.String, value[1..^1]);
            }
            else if (last == 'f' && value.Length > 1 && float.TryParse(value[0..^1], System.Globalization.NumberStyles.Float, null, out float floatNum)) // Float
            {
                return new SettingOption(name, SettingType.Float, floatNum);
            }
            else if (int.TryParse(value, out int num)) // Int
            {
                return new SettingOption(name, SettingType.Int, num);
            }
            else if (value == "On" || value == "True") // Bool 1
            {
                return new SettingOption(name, SettingType.Bool, true);
            }
            else if (value == "Off" || value == "False") // Bool 0
            {
                return new SettingOption(name, SettingType.Bool, false);
            }
            return new SettingOption(name, SettingType.RefString, value); // RefString
        }

        private static SettingOption? ParseVectorSetting(string name, string value)
        {
            string[] strings = value[1..^1].Split(',');

            float[] values = new float[strings.Length];

            for (int i = 0; i < strings.Length; i++)
            {
                if (!float.TryParse(strings[i], System.Globalization.NumberStyles.Float, null, out values[i])) return null;
            }

            switch (values.Length)
            {
                case 2: return new SettingOption(name, SettingType.Vector2, new Vector2(values[0], values[1]));
                case 3: return new SettingOption(name, SettingType.Vector3, new Vector3(values[0], values[1], values[2]));
                case 4: return new SettingOption(name, SettingType.Vector4, new Vector4(values[0], values[1], values[2], values[3]));
                default: return null;
            }
        }
    }
}
