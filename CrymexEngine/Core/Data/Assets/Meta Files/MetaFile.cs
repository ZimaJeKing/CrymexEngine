using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Text;

namespace CrymexEngine
{
    public class MetaFile
    {
        private static readonly string[] serializeIgnore =
        {
            "IncludeInRelease"
        };

        private readonly List<MetaProperty> _properties = new();

        public MetaFile(List<MetaProperty> properties)
        {
            _properties = properties;
        }

        public int? GetIntProperty(string name)
        {
            foreach (MetaProperty prop in _properties)
            {
                if (prop.name == name)
                {
                    if (!int.TryParse(prop.value, out int value)) return null;
                    return value;
                }
            }
            return null;
        }

        public float? GetFloatProperty(string name)
        {
            foreach (MetaProperty prop in _properties)
            {
                if (prop.name == name)
                {
                    if (!float.TryParse(prop.value, out float value)) return null;
                    return value;
                }
            }
            return null;
        }

        public bool? GetBoolProperty(string name)
        {
            foreach (MetaProperty prop in _properties)
            {
                if (prop.name == name)
                {
                    if (prop.value == "On" || prop.value == "True") return true;
                    else if (prop.value == "Off" || prop.value == "False") return false;
                    return null;
                }
            }
            return null;
        }

        public string? GetStringProperty(string name)
        {
            foreach (MetaProperty prop in _properties)
            {
                if (prop.name == name)
                {
                    return prop.value;
                }
            }
            return null;
        }

        public byte[] Serialize()
        {
            string formatedMeta = string.Empty;

            foreach (MetaProperty prop in _properties)
            {
                if (!serializeIgnore.Contains(prop.name))
                {
                    formatedMeta += $"{prop.name}.{prop.value};";
                }
            }
            return Encoding.Unicode.GetBytes(formatedMeta);
        }

        public static MetaFile FromSerialized(byte[] meta)
        {
            List<MetaProperty> properties = new();

            string[] stringProperties = Encoding.Unicode.GetString(meta).Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (string prop in stringProperties)
            {
                string[] split = prop.Split('.');
                if (split.Length != 2) continue;
                properties.Add(new MetaProperty(split[0], split[1]));
            }

            return new MetaFile(properties);
        }
    }

    public class MetaProperty(string name, string value)
    {
        public string name = name;
        public string value = value;
    }
}
