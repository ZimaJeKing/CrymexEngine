using CrymexEngine.Rendering;
using SixLabors.Fonts;

namespace CrymexEngine.Data
{
    public class DataAsset
    {
        public readonly string name;
        public readonly string path;

        public MetaFile? Meta => _meta;

        protected MetaFile? _meta = null;

        public DataAsset(string path)
        {
            this.path = path;
            if (Path.IsPathFullyQualified(path))
            {
                string relativePath = Path.GetRelativePath(Directories.AssetsPath, path);
                if (relativePath.StartsWith("Textures\\") || relativePath.StartsWith("Textures/")) 
                {
                    name = relativePath.Substring(9);
                }
                else if (relativePath.StartsWith("Shaders\\") || relativePath.StartsWith("Shaders/"))
                {
                    name = relativePath.Substring(8);
                }
                else if (relativePath.StartsWith("Audio\\") || relativePath.StartsWith("Audio/"))
                {
                    name = relativePath.Substring(6);
                }
                else if (relativePath.StartsWith("Scenes\\") || relativePath.StartsWith("Scenes/"))
                {
                    name = relativePath.Substring(6);
                }
                else if (relativePath.StartsWith("Fonts\\") || relativePath.StartsWith("Fonts/"))
                {
                    name = relativePath.Substring(6);
                }
                else
                {
                    name = relativePath;
                }
            }
            else if (Assets.RunningPrecompiled)
            {
                name = path;
            }
            else
            {
                Debug.LogError($"Path is in incorrect format: {path}");
                return;
            }

            if (!Assets.RunningPrecompiled)
            {
                name = name.Replace('\\', '/');
                name = RemoveExtension(name);
            }

            LoadDynamicMeta();
        }

        public void LoadDynamicMeta()
        {
            if (!Assets.UseMeta || _meta != null) return;

            string metaFilePath = path + ".meta";
            if (File.Exists(metaFilePath))
            {
                _meta = MetaFileManager.DecodeMetaFromFile(metaFilePath);
            }
        }

        private static string RemoveExtension(string path)
        {
            string[] split = path.Split('.');
            if (split.Length < 2)
            {
                Debug.LogError($"Path is in incorrect format: {path}");
                return string.Empty;
            }

            return split[0];
        }
    }

    public class TextureAsset : DataAsset
    {
        public readonly Texture texture;

        public TextureAsset(string path, Texture texture, MetaFile? meta = null) : base(path)
        {
            this.texture = texture;
            if (meta != null) _meta = meta;
        }
    }

    public class FontAsset : DataAsset
    {
        public readonly FontFamily family;

        public FontAsset(string path, FontFamily family, MetaFile? meta = null) : base(path)
        {
            this.family = family;
            if (meta != null) _meta = meta;
        }
    }

    public class AudioAsset : DataAsset
    {
        public readonly AudioClip clip;

        public AudioAsset(string path, AudioClip clip, MetaFile? meta = null) : base(path)
        {
            this.clip = clip;
            if (meta != null) _meta = meta;
        }
    }

    public class ShaderAsset : DataAsset
    {
        public readonly Shader shader;
        public readonly string vertexAssetCode;
        public readonly string fragmentAssetCode;

        public ShaderAsset(string path, Shader shader, string vertexAssetCode, string fragmentAssetCode, MetaFile? meta = null) : base(path)
        {
            this.shader = shader;
            this.vertexAssetCode = vertexAssetCode;
            this.fragmentAssetCode = fragmentAssetCode;
            if (meta != null) _meta = meta;
        }
    }
}
