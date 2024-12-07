using CrymexEngine.Debugging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

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
        private Texture(int width, int height, byte[] data)
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

        public static Texture Load(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Texture at \"{path}\" not found");
                return Missing;
            }

            using SKBitmap bitmap = SKBitmap.Decode(path);

            // Access raw pixel data
            int byteCount = bitmap.ByteCount;
            byte[] rawBytes = new byte[byteCount];
            Marshal.Copy(bitmap.GetPixels(), rawBytes, 0, byteCount);

            Texture loadedTexture = new Texture(bitmap.Width, bitmap.Height, rawBytes);

            loadedTexture.FlipY();

            return loadedTexture;
        }

        public void SetPixel(int x, int y, Color4 color)
        {
            if (x < 0 || x >= width) return;
            if  (y < 0 || y >= height) return;

            int dataIndex = (x + (y * width)) * 4;
            Color argb = Color.FromArgb(color.ToArgb());

            data[dataIndex] = argb.B;
            data[dataIndex + 1] = argb.G;
            data[dataIndex + 2] = argb.R;
            data[dataIndex + 3] = argb.A;
        }
        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= width) return;
            if (y < 0 || y >= height) return;

            int dataIndex = (x + (y * width)) * 4;

            data[dataIndex] = color.B;
            data[dataIndex + 1] = color.G;
            data[dataIndex + 2] = color.R;
            data[dataIndex + 3] = color.A;
        }

        public Color4 GetPixel(int x, int y)
        {
            x = Math.Clamp(x, 0, width - 1);
            y = Math.Clamp(y, 0, height - 1);
            byte r, g, b, a;
            int dataIndex = (x + y * width) * 4;
            b = data[dataIndex];
            g = data[dataIndex + 1];
            r = data[dataIndex + 2];
            a = data[dataIndex + 3];
            return new Color4(r, g, b, a);
        }

        public void Save(string path)
        {
            Bitmap bmp = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = (x + y * width) * 4;
                    bmp.SetPixel(x, y, Color.FromArgb(data[index + 3], data[index], data[index + 1], data[index + 2]));
                }
            }

            bmp.Save(path);
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
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data);
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
}
