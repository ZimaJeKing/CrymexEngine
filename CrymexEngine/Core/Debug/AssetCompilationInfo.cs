using CrymexEngine.Debugging;
using CrymexEngine.Utils;

namespace CrymexEngine.Debugging
{
    public class AssetCompilationInfo
    {
        public readonly float compilationTime;
        public readonly long textureCompressedSize;
        public readonly long audioCompressedSize;
        public readonly long fontSize;
        public readonly long shaderSize;
        public readonly long textSize;

        public AssetCompilationInfo(long textureCompressedSize, long audioCompressedSize, long shaderSize, long fontSize, long textSize, float compilationTime)
        {
            this.compilationTime = compilationTime;
            this.textureCompressedSize = textureCompressedSize;
            this.audioCompressedSize = audioCompressedSize;
            this.shaderSize = shaderSize;
            this.fontSize = fontSize;
            this.textSize = textSize;
        }

        public override string ToString()
        {
            string final = $"Textures: {DataUtil.ByteCountToString(UsageProfiler.TextureMemoryUsage)} raw, {DataUtil.ByteCountToString(textureCompressedSize)} compressed\n";
            final += $"Audio: {DataUtil.ByteCountToString(UsageProfiler.AudioMmeoryUsage)} raw, {DataUtil.ByteCountToString(audioCompressedSize)} compressed\n";
            final += $"Shaders: {DataUtil.ByteCountToString(shaderSize)}\n";
            final += $"Fonts: {DataUtil.ByteCountToString(fontSize)}\n";
            final += $"Text: {DataUtil.ByteCountToString(textSize)}\n";
            final += $"Compilation Time: {DataUtil.FloatToShortString(compilationTime, 3)} seconds";
            return final;
        }
    }
}
