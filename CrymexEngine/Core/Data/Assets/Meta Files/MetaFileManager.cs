namespace CrymexEngine.Data
{
    public static class MetaFileManager
    {
        public static MetaFile? GenerateMeta(DataAsset asset)
        {
            using StreamWriter writer = File.CreateText(asset.path + ".meta");

            string generatedText = string.Empty;

            generatedText += "IncludeInRelease: True\n";

            if (asset.GetType() == typeof(TextureAsset))
            {
                TextureAsset textureAsset = (TextureAsset)asset;
                generatedText += $"MaxSizeX: {textureAsset.texture.width}\n";
                generatedText += $"MaxSizeY: {textureAsset.texture.height}\n";
            }

            writer.Write(generatedText);

            return DecodeMetaFromText(generatedText);
        }

        public static MetaFile DecodeMetaFromFile(string path, DataAsset? reference = null)
        {
            if (!File.Exists(path))
            {
                if (reference != null)
                {
                    return GenerateMeta(reference);
                }
                else
                {
                    return new MetaFile(new List<MetaProperty>());
                }
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

        private static MetaFile? DecodeMetaFromText(string text)
        {
            List<MetaProperty> properties = new();
            string[] lines = text.Split('\n');
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
