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

        private readonly List<MetaProperty> _propertioes = new();

        public MetaFile(List<MetaProperty> propertioes)
        {
            _propertioes = propertioes;
        }

        /// <summary>
        /// Rerturns 0 if not found
        /// </summary>
        public int? GetIntProperty(string name)
        {
            foreach (MetaProperty prop in _propertioes)
            {
                if (prop.name == name)
                {
                    if (!int.TryParse(prop.value, out int value)) return 0;
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Rerturns NaN if not found
        /// </summary>
        public float? GetFloatProperty(string name)
        {
            foreach (MetaProperty prop in _propertioes)
            {
                if (prop.name == name)
                {
                    if (!float.TryParse(prop.value, out float value)) return float.NaN;
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Rerturns false if not found
        /// </summary>
        public bool? GetBoolProperty(string name)
        {
            foreach (MetaProperty prop in _propertioes)
            {
                if (prop.name == name)
                {
                    if (prop.value == "On" || prop.value == "True") return true;
                    else return false;
                }
            }
            return null;
        }

        /// <summary>
        /// Rerturns an empty string if not found
        /// </summary>
        public string? GetStringProperty(string name)
        {
            foreach (MetaProperty prop in _propertioes)
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

            foreach (MetaProperty prop in _propertioes)
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

    public class MetaProperty
    {
        public string name;
        public string value;

        public MetaProperty(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
