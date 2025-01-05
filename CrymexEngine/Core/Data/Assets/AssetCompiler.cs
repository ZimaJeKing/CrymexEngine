using CrymexEngine.Rendering;
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

            byte[] nameBytes = Encoding.Unicode.GetBytes(asset.name);
            byte[] data = asset.texture.CompressData(Assets.AssetTextureCompressionLevel);

            if (!LoadAssetMeta(asset, out byte[] meta)) meta = Array.Empty<byte>();

            byte[] final = new byte[4 + nameBytes.Length + 4 + data.Length + 4 + 4 + meta.Length + 4];

            using (MemoryStream memStream = new MemoryStream(final))
            {
                using BinaryWriter writer = new BinaryWriter(memStream);
                writer.Write((int)nameBytes.Length);
                writer.Write(nameBytes);
                writer.Write((int)data.Length);
                writer.Write(data);

                writer.Write(Debug.GetCheckSum(data));

                writer.Write(meta.Length);
                if (meta.Length != 0) writer.Write(meta);
                writer.Write(Debug.GetCheckSum(meta));
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

            byte[] nameBytes = Encoding.Unicode.GetBytes(asset.name);
            byte[] soundData = asset.clip.CompressData();

            if (!LoadAssetMeta(asset, out byte[] meta)) meta = Array.Empty<byte>();

            byte[] final = new byte[4 + nameBytes.Length + 4 + soundData.Length + 4 + 4 + meta.Length + 4];

            using (MemoryStream memStream = new MemoryStream(final))
            {
                using BinaryWriter writer = new BinaryWriter(memStream);
                writer.Write((int)nameBytes.Length);
                writer.Write(nameBytes);
                writer.Write((int)soundData.Length);
                writer.Write(soundData);

                writer.Write(Debug.GetCheckSum(soundData));

                writer.Write((int)meta.Length);
                if (meta.Length != 0) writer.Write(meta);
                writer.Write(Debug.GetCheckSum(meta));
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

            byte[] nameBytes = Encoding.Unicode.GetBytes(asset.name);
            byte[] vertexBytes = Encoding.Unicode.GetBytes(asset.vertexAssetCode);
            byte[] fragmentBytes = Encoding.Unicode.GetBytes(asset.fragmentAssetCode);

            if (!LoadAssetMeta(asset, out byte[] meta)) meta = Array.Empty<byte>();

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

                writer.Write(Debug.GetCheckSum(vertexBytes) + Debug.GetCheckSum(fragmentBytes));

                writer.Write(meta.Length);
                if (meta.Length != 0) writer.Write(meta);
                writer.Write(Debug.GetCheckSum(meta));
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

            byte[] nameBytes = Encoding.Unicode.GetBytes(name);

            byte[] final = new byte[4 + nameBytes.Length + 4 + data.Length + 4];

            using (MemoryStream stream = new MemoryStream(final))
            {
                using BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((int)nameBytes.Length);
                writer.Write(nameBytes);
                writer.Write((int)data.Length);
                writer.Write(data);

                writer.Write(Debug.GetCheckSum(data));
            }
            return final;
        }

        public static byte[] DecompileData(byte[] data, out string name)
        {
            using MemoryStream memStream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(memStream);

            name = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));

            int dataLength = reader.ReadInt32();
            byte[] finalData = reader.ReadBytes(dataLength);

            int checkSum = reader.ReadInt32();
            if (checkSum != Debug.GetCheckSum(finalData)) return Array.Empty<byte>();

            return finalData;
        }

        public static List<TextureAsset> DecompileTextureAssets(byte[] data)
        {
            List<TextureAsset> textureAssets = new List<TextureAsset>();
            using (MemoryStream memStream = new MemoryStream(data))
            {
                using BinaryReader reader = new BinaryReader(memStream);

                while (memStream.Position < memStream.Length - 1)
                {
                    string name = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));
                    int dataLength = reader.ReadInt32();
                    byte[] texData = reader.ReadBytes(dataLength);

                    int checkSum = reader.ReadInt32();
                    if (checkSum != Debug.GetCheckSum(texData)) continue;

                    // Load meta file
                    byte[] metaBytes = reader.ReadBytes(reader.ReadInt32());
                    MetaFile? meta;
                    if (Debug.GetCheckSum(metaBytes) != reader.ReadInt32()) meta = null;
                    else meta = MetaFile.FromSerialized(metaBytes);

                    Texture texture = Texture.FromCompressed(texData);

                    textureAssets.Add(new TextureAsset(name, texture, meta));

                    texture.Dispose();
                }
            }
            return textureAssets;
        }

        public static List<FontAsset> DecompileFontAssets(byte[] data)
        {
            List<FontAsset> fontAssets = new List<FontAsset>();
            using (MemoryStream memStream = new MemoryStream(data))
            {
                using BinaryReader reader = new BinaryReader(memStream);
                while (memStream.Position < memStream.Length - 1)
                {
                    string name = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));
                    int dataLength = reader.ReadInt32();
                    byte[] fontData = reader.ReadBytes(dataLength);

                    int checkSum = reader.ReadInt32();
                    if (checkSum != Debug.GetCheckSum(fontData)) continue;

                    Assets.LoadFontFromData(name, fontData);
                }
            }
            return fontAssets;
        }

        public static List<AudioAsset> DecompileAudioAssets(byte[] data)
        {
            List<AudioAsset> audioAssets = new List<AudioAsset>();
            using (MemoryStream memStream = new MemoryStream(data))
            {
                using BinaryReader reader = new BinaryReader(memStream);

                while (memStream.Position < memStream.Length - 1)
                {
                    int nameLen = reader.ReadInt32();
                    string name = Encoding.Unicode.GetString(reader.ReadBytes(nameLen));
                    int dataSize = reader.ReadInt32();
                    byte[] soundData = reader.ReadBytes(dataSize);

                    int checkSum = reader.ReadInt32();
                    if (checkSum != Debug.GetCheckSum(soundData)) continue;

                    // Load meta file
                    byte[] metaBytes = reader.ReadBytes(reader.ReadInt32());
                    MetaFile? meta;
                    if (Debug.GetCheckSum(metaBytes) != reader.ReadInt32()) meta = null;
                    else meta = MetaFile.FromSerialized(metaBytes);

                    audioAssets.Add(new AudioAsset(name, AudioClip.FromCompressed(soundData), meta));
                }
            }
            return audioAssets;
        }

        public static List<ShaderAsset> DecompileShaderAssets(byte[] data)
        {
            List<ShaderAsset> shaderAssets = new List<ShaderAsset>();
            using (MemoryStream memStream = new MemoryStream(data))
            {
                using BinaryReader reader = new BinaryReader(memStream);

                while (memStream.Position < memStream.Length - 1)
                {
                    string name = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));
                    byte[] vertexBytes = reader.ReadBytes(reader.ReadInt32());
                    byte[] fragmentBytes = reader.ReadBytes(reader.ReadInt32());

                    int checkSum = reader.ReadInt32();
                    if (checkSum != Debug.GetCheckSum(vertexBytes) + Debug.GetCheckSum(fragmentBytes)) continue;

                    // Load meta file
                    byte[] metaBytes = reader.ReadBytes(reader.ReadInt32());
                    MetaFile? meta;
                    if (Debug.GetCheckSum(metaBytes) != reader.ReadInt32()) meta = null;
                    else meta = MetaFile.FromSerialized(metaBytes);

                    string vertex = Encoding.Unicode.GetString(vertexBytes);
                    string fragment = Encoding.Unicode.GetString(fragmentBytes);

                    shaderAssets.Add(new ShaderAsset(name, Shader.LoadFromAsset(vertex, fragment), vertex, fragment, meta));
                }
            }
            return shaderAssets;
        }

        private static bool LoadAssetMeta(DataAsset asset, out byte[] data)
        {
            if (asset.Meta == null)
            {
                data = Array.Empty<byte>();
                return false;
            }
            else
            {
                data = asset.Meta.Serialize();
                if (!asset.Meta.GetBoolProperty("IncludeInRelease")) return false;
            }
            return true;
        }
    }
}
