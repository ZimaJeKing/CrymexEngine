using CrymexEngine.Rendering;
using CrymexEngine.UI;
using SixLabors.Fonts;

namespace CrymexEngine.Data
{
    public class DataAsset
    {
        public readonly string name;
        public readonly string path;
        public readonly bool precompiled;

        public DataAsset(string path, bool precompiled = false)
        {
            this.path = path;
            if (Path.IsPathFullyQualified(path)) name = Path.GetFileNameWithoutExtension(path);
            else name = path;
            this.precompiled = precompiled;
        }
    }

    public class TextureAsset : DataAsset
    {
        public readonly Texture texture;

        public TextureAsset(string path, Texture texture) : base(path)
        {
            this.texture = texture;
        }
    }

    public class FontAsset : DataAsset
    {
        public readonly FontFamily family;

        public FontAsset(string path, FontFamily family) : base(path)
        {
            this.family = family;
        }
    }

    public class AudioAsset : DataAsset
    {
        public readonly AudioClip clip;

        public AudioAsset(string path, AudioClip clip) : base(path)
        {
            this.clip = clip;
        }
    }

    public class ShaderAsset : DataAsset
    {
        public readonly Shader shader;
        public readonly string vertexAssetCode;
        public readonly string fragmentAssetCode;

        public ShaderAsset(string path, Shader shader, string vertexAssetCode, string fragmentAssetCode) : base(path)
        {
            this.shader = shader;
            this.vertexAssetCode = vertexAssetCode;
            this.fragmentAssetCode = fragmentAssetCode;
        }
    }
}
