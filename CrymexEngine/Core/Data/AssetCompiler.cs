using CrymexEngine.Rendering;
using System.Runtime.InteropServices;
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

            byte[] nameBytes = Encoding.Unicode.GetBytes(asset.name);
            byte[] data = asset.texture.CompressData();

            byte[] final = new byte[4 + nameBytes.Length + 4 + data.Length];

            using (MemoryStream stream = new MemoryStream(final))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((int)nameBytes.Length);
                    writer.Write(nameBytes);
                    writer.Write((int)data.Length);
                    writer.Write(data);
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

            byte[] nameBytes = Encoding.Unicode.GetBytes(asset.name);
            byte[] soundData = asset.clip.CompressData();

            byte[] final = new byte[4 + nameBytes.Length + 4 + soundData.Length];

            using (MemoryStream stream = new MemoryStream(final))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((int)nameBytes.Length);
                    writer.Write(nameBytes);
                    writer.Write((int)soundData.Length);
                    writer.Write(soundData);
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

            byte[] nameBytes = Encoding.Unicode.GetBytes(asset.name);
            byte[] vertexBytes = Encoding.Unicode.GetBytes(asset.vertexAssetCode);
            byte[] fragmentBytes = Encoding.Unicode.GetBytes(asset.fragmentAssetCode);

            byte[] final = new byte[4 + nameBytes.Length + 4 + vertexBytes.Length + 4 + fragmentBytes.Length];

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

            byte[] nameBytes = Encoding.Unicode.GetBytes(name);

            byte[] final = new byte[4 + nameBytes.Length + 4 + data.Length];

            using (MemoryStream stream = new MemoryStream(final))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((int)nameBytes.Length);
                    writer.Write(nameBytes);
                    writer.Write((int)data.Length);
                    writer.Write(data);
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
                        string vertex = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));
                        string fragment = Encoding.Unicode.GetString(reader.ReadBytes(reader.ReadInt32()));
                        shaderAssets.Add(new ShaderAsset(name, Shader.LoadFromAsset(vertex, fragment), vertex, fragment));
                    }
                }
            }
            return shaderAssets;
        }
    }
}
