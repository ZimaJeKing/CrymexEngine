using CrymexEngine.Debugging;
using CrymexEngine.Utils;

namespace CrymexEngine.Debugging
{
    public class AssetCompilationInfo
    {
        public readonly float compilationTime;
        public readonly long textureSize, audioSize, textureCompressedSize, audioCompressedSize, fontSize, shaderSize, textSize;

        public AssetCompilationInfo(long textureSize, long textureCompressedSize, long audioSize, long audioCompressedSize, long shaderSize, long fontSize, long textSize, float compilationTime)
        {
            this.compilationTime = compilationTime;
            this.textureSize = textureSize;
            this.textureCompressedSize = textureCompressedSize;
            this.audioSize = audioSize;
            this.audioCompressedSize = audioCompressedSize;
            this.shaderSize = shaderSize;
            this.fontSize = fontSize;
            this.textSize = textSize;
        }

        public override string ToString()
        {
            string final = $"Textures: {DataUtil.ByteCountToString(textureSize)} raw, {DataUtil.ByteCountToString(textureCompressedSize)} compressed\n";
            final += $"Audio: {DataUtil.ByteCountToString(audioSize)} raw, {DataUtil.ByteCountToString(audioCompressedSize)} compressed\n";
            final += $"Shaders: {DataUtil.ByteCountToString(shaderSize)}\n";
            final += $"Fonts: {DataUtil.ByteCountToString(fontSize)}\n";
            final += $"Text: {DataUtil.ByteCountToString(textSize)}\n";
            final += $"Compilation Time: {DataUtil.FloatToShortString(compilationTime, 3)} seconds";
            return final;
        }
    }
}
