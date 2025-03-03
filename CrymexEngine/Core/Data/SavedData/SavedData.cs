using CrymexEngine.Data;
using CrymexEngine.Utils;
using OpenTK.Mathematics;
using System.Text;

namespace CrymexEngine
{
    public static class SavedData
    {
        public static void WriteInt32(string name, int value)
        {
            // Remove special characters
            if (DataUtil.ContainsSpecialCharacters(name, out string newName)) name = newName;

            using (FileStream fileStream = File.OpenWrite(Directories.SaveFolderPath + name + "Int32.bin"))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write((int)value);
                }
            }
        }

        public static int? ReadInt32(string name) 
        {
            string path = Directories.SaveFolderPath + name + "Int32.bin";
            if (!File.Exists(path)) return null;

            using (FileStream fileStream =  File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    return reader.ReadInt32();
                }
            }
        }

        public static void WriteFloat(string name, float value)
        {
            // Remove special characters
            if (DataUtil.ContainsSpecialCharacters(name, out string newName)) name = newName;

            using (FileStream fileStream = File.OpenWrite(Directories.SaveFolderPath + name + "Float32.bin"))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write((float)value);
                }
            }
        }

        public static float? ReadFloat(string name)
        {
            string path = Directories.SaveFolderPath + name + "Float32.bin";
            if (!File.Exists(path)) return null;

            using (FileStream fileStream = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    return reader.ReadSingle();
                }
            }
        }

        public static void WriteString(string name, string value)
        {
            // Remove special characters
            if (DataUtil.ContainsSpecialCharacters(name, out string newName)) name = newName;

            using (FileStream fileStream = File.OpenWrite(Directories.SaveFolderPath + name + "Str.bin"))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    byte[] bytes = Encoding.Unicode.GetBytes(value);
                    writer.Write((int)bytes.Length);
                    writer.Write(bytes);
                    writer.Write((int)DataUtil.GetCheckSum(bytes));
                }
            }
        }

        public static string? ReadString(string name)
        {
            string path = Directories.SaveFolderPath + name + "Str.bin";
            if (!File.Exists(path)) return null;

            using (FileStream fileStream = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    byte[] bytes = reader.ReadBytes(reader.ReadInt32());
                    int checkSum = reader.ReadInt32();

                    if (checkSum != DataUtil.GetCheckSum(bytes)) return null;
                    return Encoding.Unicode.GetString(bytes);
                }
            }
        }

        public static void WriteBytes(string name, byte[] value)
        {
            // Remove special characters
            if (DataUtil.ContainsSpecialCharacters(name, out string newName)) name = newName;

            using (FileStream fileStream = File.OpenWrite(Directories.SaveFolderPath + name + "Bytes.bin"))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write((int)value.Length);
                    writer.Write(value);
                    writer.Write((int)DataUtil.GetCheckSum(value));
                }
            }
        }

        public static byte[]? ReadBytes(string name)
        {
            string path = Directories.SaveFolderPath + name + "Bytes.bin";
            if (!File.Exists(path)) return null;

            using (FileStream fileStream = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    byte[] bytes = reader.ReadBytes(reader.ReadInt32());
                    int checkSum = reader.ReadInt32();

                    if (checkSum != DataUtil.GetCheckSum(bytes)) return null;
                    return bytes;
                }
            }
        }

        public static void WriteVector2(string name, Vector2 value)
        {
            // Remove special characters
            if (DataUtil.ContainsSpecialCharacters(name, out string newName)) name = newName;

            using (FileStream fileStream = File.OpenWrite(Directories.SaveFolderPath + name + "Vec2.bin"))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write((float)value.X);
                    writer.Write((float)value.Y);
                }
            }
        }

        public static Vector2? ReadVector2(string name)
        {
            string path = Directories.SaveFolderPath + name + "Vec2.bin";
            if (!File.Exists(path)) return null;

            using (FileStream fileStream = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    return new Vector2(x, y);
                }
            }
        }

        public static void WriteVector3(string name, Vector3 value)
        {
            // Remove special characters
            if (DataUtil.ContainsSpecialCharacters(name, out string newName)) name = newName;

            using (FileStream fileStream = File.OpenWrite(Directories.SaveFolderPath + name + "Vec3.bin"))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write((float)value.X);
                    writer.Write((float)value.Y);
                    writer.Write((float)value.Z);
                }
            }
        }

        public static Vector3? ReadVector3(string name)
        {
            string path = Directories.SaveFolderPath + name + "Vec3.bin";
            if (!File.Exists(path)) return null;

            using (FileStream fileStream = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    return new Vector3(x, y, z);
                }
            }
        }

        public static void WriteVector4(string name, Vector4 value)
        {
            // Remove special characters
            if (DataUtil.ContainsSpecialCharacters(name, out string newName)) name = newName;

            using (FileStream fileStream = File.OpenWrite(Directories.SaveFolderPath + name + "Vec4.bin"))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write((float)value.X);
                    writer.Write((float)value.Y);
                    writer.Write((float)value.Z);
                    writer.Write((float)value.W);
                }
            }
        }

        public static Vector4? ReadVector4(string name)
        {
            string path = Directories.SaveFolderPath + name + "Vec4.bin";
            if (!File.Exists(path)) return null;

            using (FileStream fileStream = File.OpenRead(path))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    float w = reader.ReadSingle();
                    return new Vector4(x, y, z, w);
                }
            }
        }
    }
}
