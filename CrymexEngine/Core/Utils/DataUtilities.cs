using System.Text.RegularExpressions;

namespace CrymexEngine.Utils
{
    public static class DataUtilities
    {
        private static readonly string _regexPattern = @"[^\w\s]";

        /// <summary>
        /// Returns a string representation of a float trimmed to x decimal points. Uses dot character ('.') for the separator
        /// </summary>
        /// <param name="decimalPoints">A number between 1 and 10 indicating how many decimal points should be kept. </param>
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
            return split[0] + '.' + split[1];
        }

        /// <summary>
        /// Converts a number of bytes to a B, KB, MB, GB or TB string representation. 
        /// Uses multiples of 1024.
        /// Ex.: '512 KB', '2 TB', '5 MB', '14 B'
        /// </summary>
        /// <returns>A string value. Ex.: '512 KB', '2 TB', '5 MB', '14 B'</returns>
        public static string ByteCountToString(long byteCount)
        {
            int i = 0;
            while (byteCount > 1024)
            {
                byteCount /= 1024;
                if (byteCount < 1024)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                return $"{byteCount} KB";
                            }
                        case 1:
                            {
                                return $"{byteCount} MB";
                            }
                        case 2:
                            {
                                return $"{byteCount} GB";
                            }
                        case 3:
                            {
                                return $"{byteCount} TB";
                            }
                        case 4:
                            {
                                return $"{byteCount} PB";
                            }
                        case 5:
                            {
                                return $"{byteCount} EB";
                            }
                    }
                }
                i++;
            }
            return $"{byteCount} B";
        }

        /// <summary>
        /// Generates a checksum for an array of bytes
        /// </summary>
        public static int GetCheckSum(byte[] data)
        {
            if (data == null || data.Length == 0) return 0;

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

        public static string SecondsToTimeString(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            string timeString = string.Empty;

            if (time.Days > 0) timeString += time.Days + "d : ";
            if (time.Hours > 0) timeString += time.Hours + "h : ";
            if (time.Minutes > 0) timeString += time.Minutes + "m : ";

            timeString += time.Seconds + "s : ";
            timeString += time.Milliseconds + "ms";

            return timeString;
        }

        /// <summary>
        /// Returns true, if the string contains any character, that is not a word character or whitespace
        /// </summary>
        public static bool ContainsSpecialCharacters(string input)
        {
            return Regex.IsMatch(input, _regexPattern);
        }
        /// <summary>
        /// Returns true, if the string contains any character, that is not a word character or whitespace
        /// </summary>
        public static bool ContainsSpecialCharacters(string input, out string normalizedString)
        {
            if (ContainsSpecialCharacters(input))
            {
                normalizedString = Regex.Replace(input, _regexPattern, string.Empty);
                return true;
            }

            normalizedString = input;
            return false;
        }
    }
}
