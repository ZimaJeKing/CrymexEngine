using CrymexEngine.Rendering;
using CrymexEngine.Utils;
using SixLabors.Fonts;
using System.Configuration;
using System.Windows.Forms;

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
            name = DataUtil.GetCENameFromPath(path);

            LoadDynamicMeta();
        }

        public void LoadDynamicMeta()
        {
            if (!Assets.UseMeta || _meta != null || Assets.RunningPrecompiled) return;

            string metaFilePath = path + ".meta";
            if (File.Exists(metaFilePath))
            {
                _meta = MetaFileManager.DecodeMetaFromFile(metaFilePath);
            }
        }
    }

    public class TextAsset : DataAsset
    {
        public readonly string text;

        public TextAsset(string path, string text, MetaFile? meta = null) : base(path)
        {
            this.text = text;
            if (meta != null) _meta = meta;
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

    public class SettingAsset : DataAsset
    {
        public readonly Settings settings;

        public SettingAsset(string path, Settings settings) : base(path)
        {
            this.settings = settings;
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
