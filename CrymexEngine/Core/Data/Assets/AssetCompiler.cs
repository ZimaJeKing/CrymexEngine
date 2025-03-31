using CrymexEngine.Rendering;
using CrymexEngine.Utils;
using System.Text;

namespace CrymexEngine.Data
{
    public static class AssetCompiler
    {
        public static byte[] CompileTextureAsset(TextureAsset asset)
        {
            // Int32 - name length
            // bytes - name bytes
            // Int32 - data length
            // bytes - PNG compressed texture data
            // Int32 - check sum
            // Int32 - meta length
            // bytes - meta
            // Int32 - meta check sum

            using Texture texture = asset.texture;

            byte[] nameBytes = Encoding.Unicode.GetBytes(DataUtil.XorString(asset.name));
            byte[] data = texture.CompressData(Assets.TextureCompressionLevel);

            if (!SerializeAssetMeta(asset, out byte[] meta)) meta = Array.Empty<byte>();

            byte[] final = new byte[4 + nameBytes.Length + 4 + data.Length + 4 + 4 + meta.Length + 4];

            using (MemoryStream memStream = new MemoryStream(final))
            {
                using BinaryWriter writer = new BinaryWriter(memStream);
                writer.Write((int)nameBytes.Length);
                writer.Write(nameBytes);
                writer.Write((int)data.Length);
                writer.Write(data);

                writer.Write(DataUtil.GetCheckSum(data));

                writer.Write(meta.Length);
                if (meta.Length != 0) writer.Write(meta);
                writer.Write(DataUtil.GetCheckSum(meta));
            }
            return final;
        }

        public static byte[] CompileAudioAsset(AudioAsset asset)
        {
            // Int32 - name length
            // bytes - name bytes
            // Int32 - data size
            // bytes - compressed audio data
            // Int32 - check sum
            // Int32 - meta length
            // bytes - meta
            // Int32 - meta check sum

            byte[] nameBytes = Encoding.Unicode.GetBytes(DataUtil.XorString(asset.name));
            byte[] soundData = asset.clip.CompressData();

            if (!SerializeAssetMeta(asset, out byte[] meta)) meta = Array.Empty<byte>();

            byte[] final = new byte[4 + nameBytes.Length + 4 + soundData.Length + 4 + 4 + meta.Length + 4];

            using (MemoryStream memStream = new MemoryStream(final))
            {
                using BinaryWriter writer = new BinaryWriter(memStream);
                writer.Write((int)nameBytes.Length);
                writer.Write(nameBytes);
                writer.Write((int)soundData.Length);
                writer.Write(soundData);

                writer.Write(DataUtil.GetCheckSum(soundData));

                writer.Write((int)meta.Length);
                if (meta.Length != 0) writer.Write(meta);
                writer.Write(DataUtil.GetCheckSum(meta));
            }
            return final;
        }

        public static byte[] CompileShaderAsset(ShaderAsset asset)
        {
            // Int32 - name length
            // bytes - name bytes
            // Int32 - vertex length
            // bytes - vertex bytes
            // Int32 - fragment length
            // bytes - fragment bytes
            // Int32 - check sum
            // Int32 - meta length
            // bytes - meta
            // Int32 - meta check sum

            byte[] nameBytes = Encoding.Unicode.GetBytes(DataUtil.XorString(asset.name));
            byte[] vertexBytes = Encoding.Unicode.GetBytes(DataUtil.XorString(asset.vertexAssetCode));
            byte[] fragmentBytes = Encoding.Unicode.GetBytes(DataUtil.XorString(asset.fragmentAssetCode));

            if (!SerializeAssetMeta(asset, out byte[] meta)) meta = Array.Empty<byte>();

            byte[] final = new byte[4 + nameBytes.Length + 4 + vertexBytes.Length + 4 + fragmentBytes.Length + 4 + 4 + meta.Length + 4];

            using (MemoryStream memStream = new MemoryStream(final))
            {
                using BinaryWriter writer = new BinaryWriter(memStream);
                writer.Write((int)nameBytes.Length);
                writer.Write(nameBytes);
                writer.Write((int)vertexBytes.Length);
                writer.Write(vertexBytes);
                writer.Write((int)fragmentBytes.Length);
                writer.Write(fragmentBytes);

                writer.Write(DataUtil.GetCheckSum(vertexBytes) + DataUtil.GetCheckSum(fragmentBytes));

                writer.Write(meta.Length);
                if (meta.Length != 0) writer.Write(meta);
                writer.Write(DataUtil.GetCheckSum(meta));
            }
            return final;
        }

        public static byte[] CompileData(string name, byte[] data)
        {
            // Int32 - name length
            // bytes - name bytes
            // Int32 - data length
            // bytes - data bytes
            // Int32 - check sum

            byte[] nameBytes = Encoding.Unicode.GetBytes(DataUtil.XorString(name));

            byte[] final = new byte[4 + nameBytes.Length + 4 + data.Length + 4];

            using (MemoryStream stream = new MemoryStream(final))
            {
                using BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((int)nameBytes.Length);
                writer.Write(nameBytes);
                writer.Write((int)data.Length);
                writer.Write(data);

                writer.Write(DataUtil.GetCheckSum(data));
            }
            return final;
        }

        public static byte[] DecompileData(byte[] data, out string name)
        {
            using MemoryStream memStream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(memStream);

            name = DataUtil.XorString(Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32())));

            int dataLength = reader.ReadInt32();
            byte[] finalData = reader.ReadBytes(dataLength);

            int checkSum = reader.ReadInt32();
            if (checkSum != DataUtil.GetCheckSum(finalData)) return Array.Empty<byte>();

            return finalData;
        }

        internal static Dictionary<string, TextureAsset> DecompileTextureAssets(byte[] data)
        {
            Dictionary<string, TextureAsset> textureAssets = new();
            using MemoryStream memStream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(memStream);

            while (memStream.Position < memStream.Length - 1)
            {
                string name = DataUtil.XorString(Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32())));
                int dataLength = reader.ReadInt32();
                byte[] texData = reader.ReadBytes(dataLength);

                int checkSum = reader.ReadInt32();
                if (checkSum != DataUtil.GetCheckSum(texData)) continue;

                // Load meta file
                byte[] metaBytes = reader.ReadBytes(reader.ReadInt32());
                MetaFile? meta;
                if (DataUtil.GetCheckSum(metaBytes) != reader.ReadInt32()) meta = null;
                else meta = MetaFile.FromSerialized(metaBytes);

                TextureAsset asset = new TextureAsset(name, Texture.FromCompressed(texData), meta);

                textureAssets.Add(asset.name, asset);
            }
            return textureAssets;
        }

        public static Dictionary<string, FontAsset> DecompileFontAssets(byte[] data)
        {
            Dictionary<string, FontAsset> fontAssets = new();
            using (MemoryStream memStream = new MemoryStream(data))
            {
                using BinaryReader reader = new BinaryReader(memStream);
                while (memStream.Position < memStream.Length - 1)
                {
                    string name = DataUtil.XorString(Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32())));
                    int dataLength = reader.ReadInt32();
                    byte[] fontData = reader.ReadBytes(dataLength);

                    int checkSum = reader.ReadInt32();
                    if (checkSum != DataUtil.GetCheckSum(fontData)) continue;

                    Assets.LoadFontFromData(name, fontData);
                }
            }
            return fontAssets;
        }

        public static Dictionary<string, AudioAsset> DecompileAudioAssets(byte[] data)
        {
            Dictionary<string, AudioAsset> audioAssets = new();
            using (MemoryStream memStream = new MemoryStream(data))
            {
                using BinaryReader reader = new BinaryReader(memStream);

                while (memStream.Position < memStream.Length - 1)
                {
                    int nameLen = reader.ReadInt32();
                    string name = DataUtil.XorString(Encoding.Unicode.GetString(reader.ReadBytes(nameLen)));
                    int dataSize = reader.ReadInt32();
                    byte[] soundData = reader.ReadBytes(dataSize);

                    int checkSum = reader.ReadInt32();
                    if (checkSum != DataUtil.GetCheckSum(soundData)) continue;

                    // Load meta file
                    byte[] metaBytes = reader.ReadBytes(reader.ReadInt32());
                    MetaFile? meta;
                    if (DataUtil.GetCheckSum(metaBytes) != reader.ReadInt32()) meta = null;
                    else meta = MetaFile.FromSerialized(metaBytes);

                    AudioAsset asset = new AudioAsset(name, AudioClip.FromCompressed(soundData), meta);
                    audioAssets.Add(asset.name, asset);
                }
            }
            return audioAssets;
        }

        public static Dictionary<string, SettingAsset> DecompileSettingAssets(byte[] data)
        {
            Dictionary<string, SettingAsset> settingAssets = new();
            using (MemoryStream memStream = new MemoryStream(data))
            {
                using BinaryReader reader = new BinaryReader(memStream);

                while (memStream.Position < memStream.Length - 1)
                {
                    int nameLen = reader.ReadInt32();
                    string name = DataUtil.XorString(Encoding.Unicode.GetString(reader.ReadBytes(nameLen)));
                    int dataSize = reader.ReadInt32();
                    byte[] textData = reader.ReadBytes(dataSize);
                    string text = DataUtil.XorString(Encoding.Unicode.GetString(textData));

                    int checkSum = reader.ReadInt32();
                    if (checkSum != DataUtil.GetCheckSum(textData)) continue;

                    Settings settings = new Settings();
                    settings.LoadText(text);

                    SettingAsset asset = new SettingAsset(name, settings);
                    settingAssets.Add(asset.name, asset);
                }
            }
            return settingAssets;
        }

        public static Dictionary<string, ShaderAsset> DecompileShaderAssets(byte[] data)
        {
            Dictionary<string, ShaderAsset> shaderAssets = new();
            using (MemoryStream memStream = new MemoryStream(data))
            {
                using BinaryReader reader = new BinaryReader(memStream);

                while (memStream.Position < memStream.Length - 1)
                {
                    string name = DataUtil.XorString(Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32())));
                    byte[] vertexBytes = reader.ReadBytes(reader.ReadInt32());
                    byte[] fragmentBytes = reader.ReadBytes(reader.ReadInt32());

                    int checkSum = reader.ReadInt32();
                    if (checkSum != DataUtil.GetCheckSum(vertexBytes) + DataUtil.GetCheckSum(fragmentBytes)) continue;

                    // Load meta file
                    byte[] metaBytes = reader.ReadBytes(reader.ReadInt32());
                    MetaFile? meta;
                    if (DataUtil.GetCheckSum(metaBytes) != reader.ReadInt32()) meta = null;
                    else meta = MetaFile.FromSerialized(metaBytes);

                    string vertex = DataUtil.XorString(Encoding.Unicode.GetString(vertexBytes));
                    string fragment = DataUtil.XorString(Encoding.Unicode.GetString(fragmentBytes));

                    ShaderAsset asset = new ShaderAsset(name, Shader.LoadFromAsset(vertex, fragment), vertex, fragment, meta);
                    shaderAssets.Add(asset.name, asset);
                }
            }
            return shaderAssets;
        }

        private static bool SerializeAssetMeta(DataAsset asset, out byte[] data)
        {
            if (asset.Meta == null)
            {
                data = Array.Empty<byte>();
                return false;
            }
            else
            {
                data = asset.Meta.Serialize();
            }
            return true;
        }
    }
}
