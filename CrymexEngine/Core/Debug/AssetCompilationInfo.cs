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

        public AssetCompilationInfo(long textureCompressedSize, long audioCompressedSize, long shaderSize, long fontSize, float compilationTime)
        {
            this.compilationTime = compilationTime;
            this.textureCompressedSize = textureCompressedSize;
            this.audioCompressedSize = audioCompressedSize;
            this.shaderSize = shaderSize;
            this.fontSize = fontSize;
        }

        public override string ToString()
        {
            string final = $"Textures: {DataUtilities.ByteCountToString(UsageProfiler.TextureMemoryUsage)} raw, {DataUtilities.ByteCountToString(textureCompressedSize)} compressed\n";
            final += $"Audio: {DataUtilities.ByteCountToString(UsageProfiler.AudioMmeoryUsage)} raw, {DataUtilities.ByteCountToString(audioCompressedSize)} compressed\n";
            final += $"Shaders: {DataUtilities.ByteCountToString(shaderSize)}\n";
            final += $"Fonts: {DataUtilities.ByteCountToString(fontSize)}\n";
            final += $"Compilation Time: {DataUtilities.FloatToShortString(compilationTime, 3)} seconds";
            return final;
        }
    }
}
