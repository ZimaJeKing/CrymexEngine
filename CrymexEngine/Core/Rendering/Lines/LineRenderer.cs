using CrymexEngine.Rendering;
using CrymexEngine.UI;
using CrymexEngine.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class LineRenderer
    {
        public static void DrawLine(Vector2 start, Vector2 end, Color4 color, float width = 1f, float depth = 0, bool screenSpace = false)
        {
            width = Math.Clamp(width, 1f, 100f);
            depth = -Math.Clamp(depth * 0.001f, 0.001f, 1f);

            // Bind buffers
            GL.BindTexture(TextureTarget.Texture2D, Texture.White.glTexture);
            GL.BindVertexArray(Mesh.line.vao);

            Shader.Regular.Use();

            Vector2 dif = end - start;

            float rotation = MathF.Atan(dif.Y / dif.X);

            Vector2 middlePoint = start + (dif * 0.5f);

            if (!screenSpace)
            {
                middlePoint -= Camera.position;
            }
            Shader.Regular.SetParam(0, VectorUtil.Vec2ToVec3(middlePoint * Window.OneOverHalfSize, depth));

            Matrix4 transform = Matrix4.CreateRotationZ(-rotation - (MathF.PI * 0.5f))
                              * Matrix4.CreateScale(1, dif.LengthFast * Window.OneOverHalfSize.Y, 1);

            Shader.Regular.SetParam(1, transform);
            Shader.Regular.SetParam(2, color);

            GL.LineWidth(width);

            GL.DrawElements(PrimitiveType.Lines, Mesh.line.indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.UseProgram(0);
        }

        public static void DrawLines(Vector2[] positions, Color4 color, float width = 1, float depth = 0, bool screenSpace = false)
        {
            if (positions == null || positions.Length < 2) return;

            for (int i = 0; i < positions.Length - 1; i++)
            {
                DrawLine(positions[i], positions[i + 1], color, width, depth, screenSpace);
            }
        }
    }
}
