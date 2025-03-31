using CrymexEngine.Data;
using OpenTK.Graphics.OpenGL;
using System.Text;
using System.Text.RegularExpressions;
using Ude;

namespace CrymexEngine.Utils
{
    public static class DataUtil
    {
        /// <summary>
        /// Used in the 'ContainsSpecialCharacters' and 'RemoveSpecialCharacters' function
        /// </summary>
        public static readonly string regexPattern = @"[^\w\s]";
        private static readonly string[] _ceNameIgnore = [ "Textures", "Shaders", "Audio", "Fonts"];
        private static char _xorKey = '%';

        internal static void LoadSettings()
        {
            if (Settings.GlobalSettings.GetSetting("XorKeyChar", out SettingOption xorOption, SettingType.GeneralString))
            {
                string xorKey = xorOption.GetValue<string>();
                if (!string.IsNullOrEmpty(xorKey) && xorKey.Length == 1)
                {
                    _xorKey = xorKey[0];
                }
            }
        }

        /// <summary>
        /// Returns a string representation of a float trimmed to x decimal points. Uses dot character ('.') for the separator
        /// </summary>
        /// <param name="decimalPoints">A number between 1 and 10 indicating how many decimal points should be kept.</param>
        public static string FloatToShortString(float value, int decimalPoints)
        {
            decimalPoints = Math.Clamp(decimalPoints, 1, 10);
            string rawString = value.ToString();

            char separator;
            if (rawString.Contains('.')) separator = '.';
            else if (rawString.Contains(',')) separator = ',';
            else return rawString;

            string[] split = rawString.Split(separator);

            if (split.Length < 2) return rawString;
            if (split[1].Length <= decimalPoints) return split[0] + '.' + split[1];

            split[1] = split[1][0..decimalPoints];

            bool isAllZeroes = true;
            for (int i = 0; i < split[1].Length; i++)
            {
                if (split[1][i] != '0') isAllZeroes = false;
            }

            if (isAllZeroes)
            {
                return split[0];
            }

            return split[0] + '.' + split[1];
        }

        /// <summary>
        /// Converts a number of bytes to a B, KB, MB, GB or TB string representation. 
        /// Uses multiples of 1024.
        /// </summary>
        /// <returns>A string value. Ex.: '512 KB', '2 TB', '5.8 MB', '14 B'</returns>
        public static string ByteCountToString(long byteCount)
        {
            double currentByteCount = byteCount;
            int i = 0;
            while (currentByteCount > 1024)
            {
                currentByteCount /= 1024;
                if (currentByteCount < 8192)
                {
                    string byteCountString = FloatToShortString((float)currentByteCount, 1);
                    switch (i)
                    {
                        case 0: return $"{byteCountString} KB";
                        case 1: return $"{byteCountString} MB";
                        case 2: return $"{byteCountString} GB";
                        case 3: return $"{byteCountString} TB";
                        case 4: return $"{byteCountString} PB";
                        case 5: return $"{byteCountString} EB";
                    }
                }
                i++;
            }
            return $"{currentByteCount} B";
        }

        /// <summary>
        /// Generates a checksum for an array of bytes
        /// </summary>
        public static int GetCheckSum(byte[] data)
        {
            if (data == null || data.Length < 4) return 0;

            int length = data.Length - (data.Length % sizeof(int));
            int[] ints = new int[length];
            Array.Copy(data, ints, length);

            int sum = 0;

            for (int i = 0; i < length; i++)
            {
                sum += ints[i];
            }

            return sum;
        }

        /// <summary>
        /// Converts a number of seconds into a better time representation
        /// </summary>
        /// <returns>The time string. Ex. '5h : 14min : 2s : 256ms', '935ms', '5d : 57s : 435ms'</returns>
        public static string SecondsToTimeString(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            string timeString = string.Empty;

            if (time.Days > 0) timeString += time.Days + "d : ";
            if (time.Hours > 0) timeString += time.Hours + "h : ";
            if (time.Minutes > 0) timeString += time.Minutes + "min : ";
            if (time.Seconds > 0) timeString += time.Seconds + "s : ";

            timeString += time.Milliseconds + "ms";

            return timeString;
        }

        /// <summary>
        /// Performs a XOR operation on every character with the specified key character. 
        /// Original string can be brought back by running the method over it one more time with the same key.
        /// </summary>
        public static string XorString(string input, char key)
        {
            StringBuilder result = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                result.Append((char)(c ^ key));
            }

            return result.ToString();
        }
        /// <summary>
        /// Performs a XOR operation on every character with the default character specified in settings. 
        /// Original string can be brought back by running the method over the string one more time.
        /// </summary>
        public static string XorString(string input)
        {
            return XorString(input, _xorKey);
        }

        /// <summary>
        /// Returns true, if the string contains any character, that is not a word character or whitespace
        /// </summary>
        public static bool ContainsSpecialCharacters(string input)
        {
            return Regex.IsMatch(input, regexPattern);
        }

        public static void RemoveSpecialCharacters(string input, out string normalizedString)
        {
            normalizedString = Regex.Replace(input, regexPattern, string.Empty);
        }

        public static string GetCENameFromPath(string path)
        {
            string name;
            if (Path.IsPathFullyQualified(path))
            {
                name = Path.GetRelativePath(Directories.AssetsPath, path);
            }
            else
            {
                if (!Assets.RunningPrecompiled)
                {
                    Debug.LogError($"Path is in incorrect format: '{path}'");
                }
                return path;
            }

            name = name.Replace('\\', '/');
            name = RemoveExtensions(name);

            foreach (string ignore in _ceNameIgnore)
            {
                if (name.StartsWith(ignore))
                {
                    name = name[(ignore.Length + 1)..^0];
                }
            }

            return name;
        }

        public static string RemoveExtensions(string path)
        {
            int lastSeparator = path.LastIndexOf('\\');
            int dotIndex;

            if (lastSeparator == -1)
            {
                lastSeparator = path.LastIndexOf('/');

                if (lastSeparator == -1)
                {
                    if (!path.Contains('.')) return path;
                    else return Path.GetFileNameWithoutExtension(path);
                }
                else dotIndex = path.IndexOf('.', lastSeparator);
            }
            else dotIndex = path.IndexOf('.', lastSeparator);

            return path[0..dotIndex];
        }

        public static Encoding DetectTextEncoding(string path, out float confidence)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"File '{path}' doesn't exist");
                confidence = 0;
                return Encoding.UTF8;
            }

            using (FileStream fs = File.OpenRead(path))
            {
                CharsetDetector detector = new CharsetDetector();
                detector.Feed(fs);
                detector.DataEnd();
                confidence = detector.Confidence;

                try
                {
                    return Encoding.GetEncoding(detector.Charset);
                }
                catch (ArgumentException ex)
                {
                    Debug.LogError(ex.Message);
                    return Encoding.UTF8;
                }
            }
        }
    }
}
