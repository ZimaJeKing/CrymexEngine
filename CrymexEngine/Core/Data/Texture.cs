using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;

namespace CrymexEngine
{
    public class Texture : IDisposable
    {
        public byte[] data;
        public int width;
        public int height;
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

            glTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, glTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        }
        private Texture(int width, int height, byte[] data)
        {
            width = Math.Clamp(width, 1, int.MaxValue);
            height = Math.Clamp(height, 1, int.MaxValue);

            this.width = width;
            this.height = height;
            this.data = data;

            glTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, glTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Apply();
        }

        public static Texture Load(string path)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(Image.FromFile(path));
            }
            catch
            {
                Debug.Log($"Texture at \"{path}\" not found", ConsoleColor.DarkRed);
                return Missing;
            }

            Texture result = new Texture(bmp.Width, bmp.Height);

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    result.SetPixel(x, bmp.Height - y - 1, bmp.GetPixel(x, y));
                }
            }

            result.Apply();

            return result;
        }

        public void SetPixel(int x, int y, Color4 color)
        {
            if (x < 0 || x >= width) return;
            if  (y < 0 || y >= height) return;

            int dataIndex = (x + (y * width)) * 4;
            Color argb = Color.FromArgb(color.ToArgb());

            data[dataIndex] = argb.R;
            data[dataIndex + 1] = argb.G;
            data[dataIndex + 2] = argb.B;
            data[dataIndex + 3] = argb.A;
        }
        public void SetPixel(int x, int y, Color color)
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
            int size = (width * height * 4) - 1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int dataIndex = (x + (y * width)) * 4;
                    newData[size - dataIndex - 3] = data[dataIndex];
                    newData[size - dataIndex - 2] = data[dataIndex + 1];
                    newData[size - dataIndex - 1] = data[dataIndex + 2];
                    newData[size - dataIndex] = data[dataIndex + 3];
                }
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
}
