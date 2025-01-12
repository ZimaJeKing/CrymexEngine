using CrymexEngine.Debugging;

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
            string final = $"Textures: {CEUtilities.ByteCountToString(UsageProfiler.TextureMemoryUsage)} raw, {CEUtilities.ByteCountToString(textureCompressedSize)} compressed\n";
            final += $"Audio: {CEUtilities.ByteCountToString(UsageProfiler.AudioMmeoryUsage)} raw, {CEUtilities.ByteCountToString(audioCompressedSize)} compressed\n";
            final += $"Shaders: {CEUtilities.ByteCountToString(shaderSize)}\n";
            final += $"Fonts: {CEUtilities.ByteCountToString(fontSize)}\n";
            final += $"Compilation Time: {CEUtilities.FloatToShortString(compilationTime, 3)} seconds";
            return final;
        }
    }
}
