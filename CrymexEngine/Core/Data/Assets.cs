using CrymexEngine.Data;
using CrymexEngine.Rendering;
using NAudio.Wave;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class Assets
    {
        private static List<TextureAsset> _textureAssets = new();
        private static List<AudioAsset> _audioAssets = new();
        private static List<DataAsset> _scenes = new();

        public static void LoadAssets()
        {
            // Defining the white texAsset
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
            foreach (TextureAsset texture in _textureAssets)
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
            foreach (DataAsset asset in _scenes)
            {
                if (asset.name == name)
                {
                    return asset.path;
                }
            }
            return "";
        }
        public static AudioClip GetAudioClip(string name)
        {
            foreach (AudioAsset asset in _audioAssets)
            {
                if (asset.name == name)
                {
                    return asset.clip;
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

            switch (fileExtension.ToLower())
            {
                case ".png":
                    {
                        _textureAssets.Add(new TextureAsset(path, Texture.Load(path)));
                        break;
                    }
                case ".jpg":
                    {
                        _textureAssets.Add(new TextureAsset(path, Texture.Load(path)));
                        break;
                    }
                case ".bmp":
                    {
                        _textureAssets.Add(new TextureAsset(path, Texture.Load(path)));
                        break;
                    }
                case ".wav":
                    {
                        AudioClip? clip = AudioClip.Load(path);
                        if (clip == null) break;
                        _audioAssets.Add(new AudioAsset(path, clip));
                        break;
                    }
                case ".mp3":
                    {
                        AudioClip? clip = AudioClip.Load(path);
                        if (clip == null) break;
                        _audioAssets.Add(new AudioAsset(path, clip)); 
                        break;
                    }
                case ".scene":
                    {
                        _scenes.Add(new DataAsset(path));
                        break;
                    }
            }
        }

        public static void Cleanup()
        {
            foreach (AudioAsset asset in _audioAssets)
            {
                asset.clip.Dispose();
            }
            foreach (TextureAsset asset in _textureAssets)
            {
                asset.texture.Dispose();
            }
        }
    }

    class TextureAsset : DataAsset
    {
        public Texture texture;

        public TextureAsset(string path, Texture texture) : base(path)
        {
            this.texture = texture;
        }
    }

    class AudioAsset : DataAsset
    {
        public readonly AudioClip clip;

        public AudioAsset(string path, AudioClip clip) : base(path)
        {
            this.clip = clip;
        }
    }
}
