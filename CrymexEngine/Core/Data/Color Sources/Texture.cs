using CrymexEngine.Debugging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WinColor = System.Drawing.Color;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Formats.Png;
using CrymexEngine.Utils;
using CrymexEngine.Data;

namespace CrymexEngine
{
    public sealed class Texture : CEDisposable
    {
        public readonly int width;
        public readonly int height;
        public readonly int glTexture;

        private byte[] data;

        public static Texture None { get; internal set; }
        public static Texture Missing { get; internal set; }
        public static Texture White { get; internal set; }

        public Texture(int width, int height)
        {
            if (!Assets.Loaded) throw new Exception("Assets must be loaded in order to create textures.");

            // 23170 - The highest number to not overflow the data array
            width = Math.Clamp(width, 1, 23170);
            height = Math.Clamp(height, 1, 23170);

            this.width = width;
            this.height = height;

            data = new byte[width * height * 4];

            UsageProfiler.AddMemoryConsumptionValue(data.Length, MemoryUsageType.Texture);

            glTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, glTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Apply();
        }
        public Texture(int width, int height, byte[] data)
        {
            if (!Assets.Loaded) throw new Exception("Assets must be loaded in order to create textures.");

            width = Math.Clamp(width, 1, int.MaxValue);
            height = Math.Clamp(height, 1, int.MaxValue);

            this.width = width;
            this.height = height;
            this.data = data;

            UsageProfiler.AddMemoryConsumptionValue(data.Length, MemoryUsageType.Texture);

            glTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, glTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Apply();
        }

        public static Texture Load(string path, MetaFile? meta = null)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Texture at \"{path}\" not found");
                return Missing;
            }

            try
            {
                using Image<Rgba32> image = Image.Load<Rgba32>(path);

                // Access the pixel memory
                IMemoryGroup<Rgba32> pixelMemGroups = image.GetPixelMemoryGroup();

                // Allocate a byte array to hold the raw pixel data
                int byteCount = image.Width * image.Height * 4; // 4 bytes per pixel (RGBA)
                byte[] rawBytes = new byte[byteCount];

                int offset = 0;
                foreach (var memoryGroup in pixelMemGroups)
                {
                    byte[] src = MemoryMarshal.AsBytes(memoryGroup.Span).ToArray();
                    System.Buffer.BlockCopy(src, 0, rawBytes, offset, src.Length);
                    offset += src.Length;
                }

                // Create the texture
                Texture loadedTexture = new Texture(image.Width, image.Height, rawBytes);

                loadedTexture.FlipY();

                Texture finalTexture = ApplyMetaData(meta, loadedTexture);
                if (finalTexture != loadedTexture) loadedTexture.Dispose();

                return finalTexture;
            }
            catch
            {
                Debug.LogWarning($"Texture at '{path}' couldn't be loaded");
                return Missing;
            }
        }

        public void SetPixel(int x, int y, Color4 color)
        {
            if (x < 0 || x >= width) return;
            if  (y < 0 || y >= height) return;

            int dataIndex = (x + (y * width)) * 4;
            WinColor argb = WinColor.FromArgb(color.ToArgb());

            data[dataIndex] = argb.R;
            data[dataIndex + 1] = argb.G;
            data[dataIndex + 2] = argb.B;
            data[dataIndex + 3] = argb.A;
        }
        public void SetPixel(int x, int y, WinColor color)
        {
            if (x < 0 || x >= width) return;
            if (y < 0 || y >= height) return;

            int dataIndex = (x + (y * width)) * 4;

            data[dataIndex] = color.R;
            data[dataIndex + 1] = color.G;
            data[dataIndex + 2] = color.B;
            data[dataIndex + 3] = color.A;
        }

        public Color4 GetPixel(int x, int y)
        {
            x = Math.Clamp(x, 0, width - 1);
            y = Math.Clamp(y, 0, height - 1);
            byte r, g, b, a;
            int dataIndex = (x + y * width) * 4;
            r = data[dataIndex];
            g = data[dataIndex + 1];
            b = data[dataIndex + 2];
            a = data[dataIndex + 3];
            return new Color4(r, g, b, a);
        }

        public void Save(string path)
        {
            Save(path, TextureSaveFormat.PNG);
        }
        public void Save(string path, TextureSaveFormat format)
        {
            byte[] flippedData = new byte[width * height * 4];
            int stride = width * 4; // 4 bytes per pixel

            for (int row = 0; row < height; row++)
            {
                int sourceOffset = row * stride;
                int targetOffset = (height - row - 1) * stride;
                Array.Copy(data, sourceOffset, flippedData, targetOffset, stride);
            }

            Image<Rgba32> isImage = Image.LoadPixelData<Rgba32>(flippedData, width, height);

            switch (format)
            {
                case TextureSaveFormat.PNG:
                    {
                        isImage.SaveAsPng(path);
                        break;
                    }
                case TextureSaveFormat.BMP:
                    {
                        isImage.SaveAsBmp(path);
                        break;
                    }
                case TextureSaveFormat.JPEG:
                    {
                        isImage.SaveAsJpeg(path);
                        break;
                    }
                case TextureSaveFormat.GIF:
                    {
                        isImage.SaveAsGif(path);
                        break;
                    }
            }
        }

        public byte[] CompressData(int level)
        {
            level = Math.Clamp(level, 0, 9);

            Image<Rgba32> isImage = Image.LoadPixelData<Rgba32>(data, width, height);

            // Save the Image as PNG in a MemoryStream
            using MemoryStream memoryStream = new MemoryStream();
            isImage.Save(memoryStream, new PngEncoder() { CompressionLevel = (PngCompressionLevel)level });

            return memoryStream.ToArray();
        }

        public static Texture FromCompressed(byte[] data, MetaFile? meta = null)
        {
            using MemoryStream memoryStream = new MemoryStream(data);
            using Image<Rgba32> image = Image.Load<Rgba32>(memoryStream);

            Texture loaded = FromImageSharp(image);
            Texture final = ApplyMetaData(meta, loaded);
            if (final != loaded) loaded.Dispose();

            return final;
        }

        public static Texture FromImageSharp(Image<Rgba32> image)
        {
            // Access the pixel memory
            IMemoryGroup<Rgba32> pixelMemGroups = image.GetPixelMemoryGroup();

            // Allocate a byte array to hold the raw pixel data
            byte[] rawBytes = new byte[image.Width * image.Height * 4];

            int offset = 0;
            foreach (var memoryGroup in pixelMemGroups)
            {
                byte[] src = MemoryMarshal.AsBytes(memoryGroup.Span).ToArray();
                System.Buffer.BlockCopy(src, 0, rawBytes, offset, src.Length);
                offset += src.Length;
            }

            Texture texture = new Texture(image.Width, image.Height, rawBytes);
            return texture;
        }

        /// <summary>
        /// Flips the texture upside down and calls Texture.Apply()
        /// </summary>
        public void FlipY()
        {
            byte[] newData = new byte[data.Length];
            int stride = width * 4; // 4 bytes per pixel

            for (int row = 0; row < height; row++)
            {
                int sourceOffset = row * stride;
                int targetOffset = (height - row - 1) * stride;
                Array.Copy(data, sourceOffset, newData, targetOffset, stride);
            }

            data = newData;
            Apply();
        }

        /// <summary>
        /// Flips the texture left to right and calls Texture.Apply()
        /// </summary>
        public void FlipX()
        {
            byte[] newData = new byte[data.Length];
            int size = (width * height * 4) - 1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dataIndex = (x + (y * width)) * 4;
                    int flippedDataIndex = (width - x - 1 + (y * width)) * 4;
                    newData[flippedDataIndex] = data[dataIndex];
                    newData[flippedDataIndex + 1] = data[dataIndex + 1];
                    newData[flippedDataIndex + 2] = data[dataIndex + 2];
                    newData[flippedDataIndex + 3] = data[dataIndex + 3];
                }
            }

            data = newData;
            Apply();
        }

        /// <returns>A copy of the raw data in RGBA32 format. Ordered from top left</returns>
        public byte[] GetRawData()
        {
            byte[] rawData = new byte[data.Length];
            data.CopyTo(rawData, 0);
            return rawData;
        }

        /// <summary>
        /// Sets the raw data for this texture. RGBA32 format ordered from top left
        /// </summary>
        public void SetRawData(byte[] rawData)
        {
            if (rawData.Length != data.Length)
            {
                Debug.LogError($"Raw texture data size is the wrong length. Expected {DataUtil.ByteCountToString(data.Length)}, got {DataUtil.ByteCountToString(rawData.Length)}");
            }
        }

        /// <summary>
        /// Applies changes made to the pixel data
        /// </summary>
        public void Apply()
        {
            GL.BindTexture(TextureTarget.Texture2D, glTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Texture Clone()
        {
            return new Texture(width, height, data);
        }
        public Texture Resize(int newWidth, int newHeight)
        {
            if (newWidth < 1 || newHeight < 1 || newWidth > 10000 || newHeight > 10000) return Missing;

            Texture texture = new Texture(newWidth, newHeight);

            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, GetPixel((int)(x / (float)newWidth * width), (int)(y / (float)newHeight * height)));
                }
            }

            texture.Apply();

            return texture;
        }

        protected override void OnDispose()
        {
            GL.DeleteBuffer(glTexture);

            UsageProfiler.AddMemoryConsumptionValue(-data.Length, MemoryUsageType.Texture);
        }

        private static Texture ApplyMetaData(MetaFile? metadata, Texture texture)
        {
            if (metadata == null || !Assets.UseMeta) return texture;

            int? maxX = metadata.GetIntProperty("MaxSizeX");
            int? maxY = metadata.GetIntProperty("MaxSizeY");

            if (maxX != null && maxY != null && maxX > 0 && maxY > 0)
            {
                if (maxX < texture.width && maxY < texture.height)
                {
                    Texture original = texture; 
                    texture = original.Resize(maxX.Value, maxY.Value);
                    original.Dispose();
                }
            }
            return texture;
        }
    }

    public enum TextureSaveFormat { PNG, BMP, JPEG, GIF }
}
