using CrymexEngine.Rendering;
using NAudio.Wave;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace CrymexEngine
{
    public static class Assets
    {
        private static List<TexAsset> textures = new List<TexAsset>();
        private static List<AudioClip> audioClips = new List<AudioClip>();
        private static List<SceneAsset> scenes = new List<SceneAsset>();

        public static void LoadAssets()
        {
            // Defining the white texture
            Texture white = new Texture(1, 1);
            white.SetPixel(0, 0, Color4.White);
            white.Apply();
            Texture.White = white;

            Texture none = new Texture(1, 1);
            none.SetPixel(0, 0, Color4.Transparent);
            none.Apply();
            Texture.None = none;

            // Starts a recursive loop of searching directories in the "Assets" folder
            // Responsible for loading all assets
            SearchDirectory(Debug.assetsPath);

            Texture.Missing = GetTexture("Missing");

            Shader.LoadDefaultShaders();
        }

        public static Texture GetTexture(string name)
        {
            foreach (TexAsset texture in textures)
            {
                if (texture.name == name)
                {
                    return texture.texture;
                }
            }
            return Texture.Missing;
        }
        public static string GetScenePath(string name)
        {
            foreach (SceneAsset scene in scenes)
            {
                if (scene.name == name)
                {
                    return scene.path;
                }
            }
            return "";
        }
        public static AudioClip GetAudioClip(string name)
        {
            foreach (AudioClip clip in audioClips)
            {
                if (clip.name == name)
                {
                    return clip;
                }
            }
            return null;
        }

        private static void SearchDirectory(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);

            for (int i = 0; i < files.Length; i++)
            {
                LoadAsset(files[i]);
            }
            for (int i = 0; i < directories.Length; i++)
            {
                SearchDirectory(directories[i]);
            }
        }

        private static void LoadAsset(string path)
        {
            string fileExtension = Path.GetExtension(path);
            string fileName = Path.GetFileNameWithoutExtension(path);

            switch (fileExtension.ToLower())
            {
                case ".png":
                    {
                        textures.Add(new TexAsset(fileName, path, Texture.Load(path)));
                        break;
                    }
                case ".jpg":
                    {
                        textures.Add(new TexAsset(fileName, path, Texture.Load(path)));
                        break;
                    }
                case ".bmp":
                    {
                        textures.Add(new TexAsset(fileName, path, Texture.Load(path)));
                        break;
                    }
                case ".wav":
                    {
                        audioClips.Add(new AudioClip(fileName, LoadSound(path, out WaveFormat format, out int size), format, size));
                        break;
                    }
                case ".mp3":
                    {
                        audioClips.Add(new AudioClip(fileName, LoadSound(path, out WaveFormat format, out int size), format, size));
                        break;
                    }
                case ".scene":
                    {
                        scenes.Add(new SceneAsset(fileName, path));
                        break;
                    }
            }
        }

        public static IntPtr LoadSound(string path, out WaveFormat format, out int size)
        {
            size = 0;
            IntPtr unmanagedBuffer;
            using (AudioFileReader reader = new AudioFileReader(path))
            {
                format = new WaveFormat(44100, 16, 2);

                using (MediaFoundationResampler resampler = new MediaFoundationResampler(reader, format))
                {
                    resampler.ResamplerQuality = 60;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;

                        while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memoryStream.Write(buffer, 0, bytesRead);
                        }

                        byte[] fullData = memoryStream.ToArray();
                        unmanagedBuffer = Marshal.AllocHGlobal(fullData.Length);

                        Marshal.Copy(fullData, 0, unmanagedBuffer, fullData.Length);
                        size = fullData.Length;
                    }
                }
            }
            return unmanagedBuffer;
        }

        public static void Cleanup()
        {
            for (int i = 0; i < audioClips.Count; i++)
            {
                Marshal.FreeHGlobal(audioClips[i].soundData);
            }
            foreach (TexAsset texture in textures)
            {
                GL.DeleteBuffer(texture.texture.glTexture);
            }
        }
    }

    class TexAsset
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

    class SceneAsset
    {
        public string name;
        public string path;

        public SceneAsset(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
    }
}
