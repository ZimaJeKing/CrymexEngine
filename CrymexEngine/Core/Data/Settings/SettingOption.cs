using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrymexEngine
{
    public class SettingOption
    {
        public string name;
        public SettingType type;
        public object value;

        public SettingOption(string name, SettingType type, object value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }

        public T GetValue<T>()
        {
            Type tType = typeof(T);
            if (!TypeMatch(type, tType)) return default;

            if (typeof(T) == typeof(Color4))
            {
                byte[] bytes = BitConverter.GetBytes((int)value);
                return (T)(object)new Color4(bytes[0], bytes[1], bytes[2], 255);
            }

            return (T)value;
        }

        private static bool TypeMatch(SettingType settingType, Type Ttype)
        {
            if (Ttype == typeof(int) || Ttype == typeof(float))
            {
                if (settingType == SettingType.Int || settingType == SettingType.Float)
                {
                    return true;
                }
            }
            if (Ttype == typeof(string))
            {
                if (settingType == SettingType.String || settingType == SettingType.RefString)
                {
                    return true;
                }
            }
            if (settingType == SettingType.Hex)
            {
                if (Ttype == typeof(Color4) || Ttype == typeof(Vector4) || Ttype == typeof(int))
                {
                    return true;
                }
            }
            if (Ttype == typeof(Vector2) && settingType == SettingType.Vector2)
            {
                return true;
            }
            if (Ttype == typeof(Vector3) && settingType == SettingType.Vector3)
            {
                return true;
            }
            if (Ttype == typeof(Vector4) && settingType == SettingType.Vector4)
            {
                return true;
            }
            if (Ttype == typeof(bool) && settingType == SettingType.Bool)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            string final = name + ": ";
            string? strValue = value.ToString();
            if (strValue == null) return "";

            if (type == SettingType.Float) strValue += 'f';
            else if (type == SettingType.Hex) strValue = '#' + strValue;
            else if (type == SettingType.String) strValue = '"' + strValue + '"';
            else if (type == SettingType.Bool)
            {
                if ((bool)value == true)
                {
                    strValue = "True";
                }
                else
                {
                    strValue = "False";
                }
            }

            final += strValue;
            return final;
        }
    }

    public enum SettingType { None, Int, Float, String, RefString, Hex, Vector2, Vector3, Vector4, Bool }
}
