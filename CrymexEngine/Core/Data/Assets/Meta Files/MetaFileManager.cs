namespace CrymexEngine.Data
{
    internal static class MetaFileManager
    {
        private static MetaFile GenerateMeta(string path)
        {
            using StreamWriter writer = File.CreateText(path);

            string generatedText = string.Empty;

            generatedText += "IncludeInRelease: True";

            writer.Write(generatedText);

            return new MetaFile(new List<MetaProperty>() { new MetaProperty("IncludeInRelease", "True") });
        }

        internal static MetaFile DecodeMetaFromFile(string path)
        {
            if (!Assets.UseMeta) return null;

            if (!File.Exists(path))
            {
                return GenerateMeta(path);
            }

            List<MetaProperty> properties = new();
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] split = line.Split(':', StringSplitOptions.TrimEntries);
                if (split.Length != 2) continue;
                properties.Add(new MetaProperty(split[0], split[1]));
            }
            return new MetaFile(properties);
        }
    }
}
