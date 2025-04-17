using CrymexEngine.Data;
using CrymexEngine.Utils;
using OpenTK.Mathematics;
using System.Text;

namespace CrymexEngine
{
    public static class SavedData
    {
        private static void WriteData<T>(string name, T value, string extension, Action<BinaryWriter, T> writeAction)
        {
            name = DataUtil.NormalizeName(name, 20);

            DataUtil.RemoveSpecialCharacters(name, out name);
            string path = Directories.SaveFolderPath + name + extension;

            using (FileStream fileStream = File.Create(path))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                writeAction(writer, value);
            }
        }

        private static T? ReadData<T>(string name, string extension, Func<BinaryReader, T> readAction)
        {
            string path = Directories.SaveFolderPath + name + extension;
            if (!File.Exists(path)) return default;

            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    return readAction(reader);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occurred while reading saved data '{name}': {e.Message}");
                return default;
            }
        }

        public static void WriteInt32(string name, int value) =>
            WriteData(name, value, "Int32.bin", (writer, val) => writer.Write(val));

        public static int? ReadInt32(string name) =>
            ReadData(name, "Int32.bin", reader => reader.ReadInt32());

        public static void WriteFloat(string name, float value) =>
            WriteData(name, value, "Float32.bin", (writer, val) => writer.Write(val));

        public static float? ReadFloat(string name) =>
            ReadData(name, "Float32.bin", reader => reader.ReadSingle());

        public static void WriteString(string name, string value)
        {
            WriteData(name, value, "Str.bin", (writer, val) =>
            {
                byte[] bytes = Encoding.Unicode.GetBytes(DataUtil.XorString(val));
                writer.Write(bytes.Length);
                writer.Write(bytes);
                writer.Write(DataUtil.GetCheckSum(bytes));
            });
        }

        public static string? ReadString(string name)
        {
            return ReadData(name, "Str.bin", reader =>
            {
                byte[] bytes = reader.ReadBytes(reader.ReadInt32());
                int checkSum = reader.ReadInt32();
                if (checkSum != DataUtil.GetCheckSum(bytes)) return null;
                return DataUtil.XorString(Encoding.Unicode.GetString(bytes));
            });
        }

        public static void WriteBytes(string name, byte[] value) =>
            WriteData(name, value, "Bytes.bin", (writer, val) =>
            {
                writer.Write(val.Length);
                writer.Write(val);
                writer.Write(DataUtil.GetCheckSum(val));
            });

        public static byte[]? ReadBytes(string name) =>
            ReadData(name, "Bytes.bin", reader =>
            {
                byte[] bytes = reader.ReadBytes(reader.ReadInt32());
                int checkSum = reader.ReadInt32();
                return checkSum == DataUtil.GetCheckSum(bytes) ? bytes : null;
            });

        public static void WriteVector2(string name, Vector2 value) =>
            WriteData(name, value, "Vec2.bin", (writer, val) =>
            {
                writer.Write(val.X);
                writer.Write(val.Y);
            });

        public static Vector2? ReadVector2(string name) =>
            ReadData(name, "Vec2.bin", reader => new Vector2(reader.ReadSingle(), reader.ReadSingle()));

        public static void WriteVector3(string name, Vector3 value) =>
            WriteData(name, value, "Vec3.bin", (writer, val) =>
            {
                writer.Write(val.X);
                writer.Write(val.Y);
                writer.Write(val.Z);
            });

        public static Vector3? ReadVector3(string name) =>
            ReadData(name, "Vec3.bin", reader => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));

        public static void WriteVector4(string name, Vector4 value) =>
            WriteData(name, value, "Vec4.bin", (writer, val) =>
            {
                writer.Write(val.X);
                writer.Write(val.Y);
                writer.Write(val.Z);
                writer.Write(val.W);
            });

        public static Vector4? ReadVector4(string name) =>
            ReadData(name, "Vec4.bin", reader => new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));

        public static void WriteTexture(string name, Texture value)
        {
            if (value.Disposed) throw new ArgumentException(name + " is disposed. Cannot save texture data.");
            byte[] texturebytes = value.CompressData(Assets.TextureCompressionLevel);
            WriteData(name, texturebytes, "Tex.bin", (writer, val) =>
            {
                writer.Write(val.Length);
                writer.Write(val);
                writer.Write(DataUtil.GetCheckSum(val));
            });
        }

        public static Texture? ReadTexture(string name) =>
            ReadData(name, "Tex.bin", reader =>
            {
                byte[] bytes = reader.ReadBytes(reader.ReadInt32());
                int checkSum = reader.ReadInt32();
                return checkSum == DataUtil.GetCheckSum(bytes) ? Texture.FromCompressed(bytes) : null;
            });

        public static void WriteColor(string name, Color4 value) => WriteData(name, value, "Color.bin", (writer, val) =>
        {
            writer.Write(val.R);
            writer.Write(val.G);
            writer.Write(val.B);
            writer.Write(val.A);
        });

        public static Color4 ReadColor(string name) =>
            ReadData(name, "Color.bin", reader => new Color4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));

        public static void WriteIntArray(string name, int[] value)
        {
            WriteData(name, value, "Int32Arr.bin", (writer, val) =>
            {
                writer.Write(val.Length);
                foreach (int i in val)
                {
                    writer.Write(i);
                }
            });
        }

        public static int[]? ReadIntArray(string name)
        {
            return ReadData(name, "Int32Arr.bin", reader =>
            {
                int length = reader.ReadInt32();
                int[] array = new int[length];
                for (int i = 0; i < length; i++)
                {
                    array[i] = reader.ReadInt32();
                }
                return array;
            });
        }

        public static void WriteFloatArray(string name, float[] value)
        {
            WriteData(name, value, "Float32Arr.bin", (writer, val) =>
            {
                writer.Write(value.Length);
                foreach (float i in value)
                {
                    writer.Write(i);
                }
            });
        }

        public static float[]? ReadFloatArray(string name)
        {
            return ReadData(name, "Float32Arr.bin", reader =>
            {
                int length = reader.ReadInt32();
                float[] array = new float[length];
                for (int i = 0; i < length; i++)
                {
                    array[i] = reader.ReadSingle();
                }
                return array;
            });
        }

        public static void WriteColorArray(string name, Color4[] value)
        {
            WriteData(name, value, "ColorArr.bin", (writer, val) =>
            {
                writer.Write(value.Length);
                foreach (Color4 i in value)
                {
                    writer.Write(i.R);
                    writer.Write(i.G);
                    writer.Write(i.B);
                    writer.Write(i.A);
                }
            });
        }

        public static Color4[]? ReadColorArray(string name)
        {
            return ReadData(name, "ColorArr.bin", reader =>
            {
                int length = reader.ReadInt32();
                Color4[] array = new Color4[length];
                for (int i = 0; i < length; i++)
                {
                    array[i] = new Color4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }
                return array;
            });
        }
    }
}
