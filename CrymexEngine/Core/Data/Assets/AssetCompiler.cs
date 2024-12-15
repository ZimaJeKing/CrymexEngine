using CrymexEngine.Rendering;
using System.Runtime.InteropServices;
using System.Text;

namespace CrymexEngine.Data
{
    public static class AssetCompiler
    {
        public static bool compareCheckSum = true;

        public static byte[] CompileTextureAsset(TextureAsset asset)
        {
            // Int32 - name length
            // bytes - name bytes
            // Int32 - data length
            // bytes - PNG compressed texture data
            // Int32 - check sum

            byte[] nameBytes = Encoding.Unicode.GetBytes(asset.name);
            byte[] data = asset.texture.CompressData(Assets.AssetTextureCompressionLevel);

            byte[] final = new byte[4 + nameBytes.Length + 4 + data.Length + 4];

            using (MemoryStream stream = new MemoryStream(final))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((int)nameBytes.Length);
                    writer.Write(nameBytes);
                    writer.Write((int)data.Length);
                    writer.Write(data);

                    writer.Write(Debug.GetCheckSum(data));
                }
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

            byte[] nameBytes = Encoding.Unicode.GetBytes(asset.name);
            byte[] soundData = asset.clip.CompressData();

            byte[] final = new byte[4 + nameBytes.Length + 4 + soundData.Length + 4];

            using (MemoryStream stream = new MemoryStream(final))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((int)nameBytes.Length);
                    writer.Write(nameBytes);
                    writer.Write((int)soundData.Length);
                    writer.Write(soundData);

                    writer.Write(Debug.GetCheckSum(soundData));
                }
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

            byte[] nameBytes = Encoding.Unicode.GetBytes(asset.name);
            byte[] vertexBytes = Encoding.Unicode.GetBytes(asset.vertexAssetCode);
            byte[] fragmentBytes = Encoding.Unicode.GetBytes(asset.fragmentAssetCode);

            byte[] final = new byte[4 + nameBytes.Length + 4 + vertexBytes.Length + 4 + fragmentBytes.Length + 4];

            using (MemoryStream stream = new MemoryStream(final))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((int)nameBytes.Length);
                    writer.Write(nameBytes);
                    writer.Write((int)vertexBytes.Length);
                    writer.Write(vertexBytes);
                    writer.Write((int)fragmentBytes.Length);
                    writer.Write(fragmentBytes);

                    writer.Write(Debug.GetCheckSum(vertexBytes) + Debug.GetCheckSum(fragmentBytes));
                }
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
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((int)nameBytes.Length);
                    writer.Write(nameBytes);
                    writer.Write((int)data.Length);
                    writer.Write(data);

                    writer.Write(Debug.GetCheckSum(data));
                }
            }
            return final;
        }

        public static byte[] DecompileData(byte[] data, out string name)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    name = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));

                    int dataLength = reader.ReadInt32();
                    byte[] finalData = reader.ReadBytes(dataLength);

                    int checkSum = reader.ReadInt32();
                    if (compareCheckSum && checkSum != Debug.GetCheckSum(finalData)) return new byte[0];

                    return finalData;
                }
            }
        }

        public static List<TextureAsset> DecompileTextureAssets(byte[] data)
        {
            List<TextureAsset> textureAssets = new List<TextureAsset>();
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (stream.Position < stream.Length - 1)
                    {
                        string name = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));
                        int dataLength = reader.ReadInt32();
                        byte[] texData = reader.ReadBytes(dataLength);

                        int checkSum = reader.ReadInt32();
                        if (compareCheckSum && checkSum != Debug.GetCheckSum(texData)) continue;

                        textureAssets.Add(new TextureAsset(name, Texture.FromCompressed(texData)));
                    }
                }
            }
            return textureAssets;
        }

        public static List<AudioAsset> DecompileAudioAssets(byte[] data)
        {
            List<AudioAsset> audioAssets = new List<AudioAsset>();
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (stream.Position < stream.Length - 1)
                    {
                        int nameLen = reader.ReadInt32();
                        string name = Encoding.Unicode.GetString(reader.ReadBytes(nameLen));
                        int dataSize = reader.ReadInt32();
                        byte[] soundData = reader.ReadBytes(dataSize);

                        int checkSum = reader.ReadInt32();
                        if (compareCheckSum && checkSum != Debug.GetCheckSum(soundData)) continue;

                        audioAssets.Add(new AudioAsset(name, AudioClip.FromCompressed(soundData)));
                    }
                }
            }
            return audioAssets;
        }

        public static List<ShaderAsset> DecompileShaderAssets(byte[] data)
        {
            List<ShaderAsset> shaderAssets = new List<ShaderAsset>();
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (stream.Position < stream.Length - 1)
                    {
                        string name = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));
                        byte[] vertexBytes = reader.ReadBytes(reader.ReadInt32());
                        byte[] fragmentBytes = reader.ReadBytes(reader.ReadInt32());

                        int checkSum = reader.ReadInt32();
                        if (compareCheckSum && checkSum != Debug.GetCheckSum(vertexBytes) + Debug.GetCheckSum(fragmentBytes)) continue;

                        string vertex = Encoding.Unicode.GetString(vertexBytes);
                        string fragment = Encoding.Unicode.GetString(fragmentBytes);

                        shaderAssets.Add(new ShaderAsset(name, Shader.LoadFromAsset(vertex, fragment), vertex, fragment));
                    }
                }
            }
            return shaderAssets;
        }
    }
}
