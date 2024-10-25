using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CrymexEngine
{
    public static class Assets
    {
        private static TexAsset[] textures = new TexAsset[0];

        public static void LoadTextures()
        {
            Texture _wt = new Texture(1, 1);
            _wt.SetPixel(0, 0, Color4.White);
            _wt.Apply();
            Texture.White = _wt;

            Texture none = new Texture(1, 1);
            none.SetPixel(0, 0, Color4.Transparent);
            none.Apply();
            Texture.None = none;

            Texture.Missing = Texture.Load(Debug.assetsPath + "Textures\\Missing.png");

            List<TexAsset> list = new List<TexAsset>();

            foreach (string path in Directory.GetFiles(Debug.assetsPath + "Textures"))
            {
                list.Add(new TexAsset(Path.GetFileNameWithoutExtension(path), path, Texture.Load(path)));
            }

            textures = list.ToArray();
        }

        public static Texture GetTexture(string name)
        {
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i].name == name)
                {
                    return textures[i].texture;
                }
            }
            return Texture.Missing;
        }

        private static string[] settings = new string[0];
        public static void LoadSettings()
        {
            string globalSettingsPath = Debug.assetsPath + "GlobalSettings.txt";
            if (!File.Exists(globalSettingsPath)) File.Create(globalSettingsPath).Close();

            settings = File.ReadAllLines(globalSettingsPath);
        }

        public static unsafe void ApplyPostSettings()
        {
            for (int i = 0; i < settings.Length; i++)
            {
                if (settings[i] == "") continue;
                string line = settings[i].Trim();
                if (line.Substring(0, 2) == "//") continue;
                string[] split = line.Split(':', StringSplitOptions.TrimEntries);
                if (split.Length < 2) continue;
                split[1] = split[1].Trim();

                switch (split[0])
                {
                    case "VSync":
                        {
                            if (split[1] == "On") Program.window.VSync = VSyncMode.On;
                            else if (split[1] == "Adaptive") Program.window.VSync = VSyncMode.Adaptive;
                            else Program.window.VSync = VSyncMode.Off;
                            break;
                        }
                    case "WindowSize":
                        {
                            string[] nums = split[1].Split("x");
                            if (nums.Length != 2) { Debug.LogL("[Settings] Wrong window size format"); continue; }

                            if (!int.TryParse(nums[0], out int x)) { if (Program.debugMode) Debug.LogL("[Settings] Wrong window size format", ConsoleColor.DarkRed); continue; }
                            if (!int.TryParse(nums[1], out int y)) { if (Program.debugMode) Debug.LogL("[Settings] Wrong window size format", ConsoleColor.DarkRed); continue; }

                                Program.window.Size = new Vector2i(x, y);
                            break;
                        }
                    case "WindowIcon":
                        {
                            Program.windowIcon = GetTexture(split[1]);
                            break;
                        }
                    case "WindowTitle":
                        {
                            Program.window.Title = split[1];
                            break;
                        }
                    case "DebugMode":
                        {
                            if (split[1] == "On") Program.debugMode = true;
                            break;
                        }
                    case "RenderDistance":
                        {
                            if (!int.TryParse(split[1], out int renderDist)) break;
                            Camera.renderDistance = renderDist;
                            break;
                        }
                    case "ClearColor":
                        {
                            if (split.Length < 2 || !int.TryParse(split[1].Substring(1), System.Globalization.NumberStyles.HexNumber, null, out int num)) break;
                            Camera.clearColor = Color.FromArgb(num);
                            break;
                        }
                    case "WindowResizeable":
                        {
                            if (split[1] == "False") Program.windowResizeable = false;
                            else Program.windowResizeable = true;
                            break;
                        }
                }
            }
            
        }
    }

    public class TexAsset
    {
        public string name;
        public string path;
        public Texture texture;

        public TexAsset(string name, string path, Texture texture)
        {
            this.name = name;
            this.path = path;
            this.texture = texture;
        }
    }
}
