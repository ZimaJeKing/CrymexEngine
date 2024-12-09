using CrymexEngine.Debugging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WinColor = System.Drawing.Color;
using ISColor = SixLabors.ImageSharp.Color;
using System;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Advanced;
using System.Security.AccessControl;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;

namespace CrymexEngine
{
    public class Texture : IDisposable
    {
        public byte[] data;
        public readonly int width;
        public readonly int height;
        public int glTexture;

        public static Texture None;
        public static Texture Missing;
        public static Texture White;

        public Texture(int width, int height)
        {
            width = Math.Clamp(width, 1, int.MaxValue);
            height = Math.Clamp(height, 1, int.MaxValue);

            this.width = width;
            this.height = height;

            data = new byte[width * height * 4];

            UsageProfiler.AddDataConsumptionValue(data.Length, MemoryUsageType.Texture);

            glTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, glTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        }
        public Texture(int width, int height, byte[] data)
        {
            width = Math.Clamp(width, 1, int.MaxValue);
            height = Math.Clamp(height, 1, int.MaxValue);

            this.width = width;
            this.height = height;
            this.data = data;

            UsageProfiler.AddDataConsumptionValue(data.Length, MemoryUsageType.Texture);

            glTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, glTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Apply();
        }

        public static unsafe Texture Load(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Texture at \"{path}\" not found");
                return Missing;
            }

            using (Image<Rgba32> image = Image.Load<Rgba32>(path))
            {
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

                return loadedTexture;
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

        public byte[] CompressData()
        {
            Image<Rgba32> isImage = Image.LoadPixelData<Rgba32>(data, width, height);

            // Save the Image as PNG in a MemoryStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                isImage.Save(memoryStream, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression });

                return memoryStream.ToArray();
            }
        }

        public static Texture FromCompressed(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (Image<Rgba32> image = Image.Load<Rgba32>(memoryStream))
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

                    return new Texture(image.Width, image.Height, rawBytes);
                }
            }
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

        /// <summary>
        /// Applies changes made to the pixel data
        /// </summary>
        public void Apply()
        {
            GL.BindTexture(TextureTarget.Texture2D, glTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        public Texture Clone()
        {
            return new Texture(width, height, data);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(glTexture);
        }
    }

    public enum TextureSaveFormat { PNG, BMP, JPEG, GIF }
}
