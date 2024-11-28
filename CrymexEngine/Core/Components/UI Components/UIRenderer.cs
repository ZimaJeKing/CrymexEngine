using CrymexEngine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace CrymexEngine.UI
{
    public class UIRenderer : UIComponent
    {
        public Texture texture;
        public Shader shader;
        public Color color;

        public static void Init()
        {
            
        }

        public override void Update()
        {
            if (enabled)
            {
                // Bind texture
                GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);
                GL.BindVertexArray(Mesh.quad.vao);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, Mesh.quad.ebo);
                GL.UseProgram(shader._glShader);

                // Set first three shader parameters for position, transformation, and color
                shader.SetParam(0, (UIElement.position - Camera.position) / (Window.Size.ToVector2() * 0.5f));
                shader.SetParam(1, UIElement.scaleMatrix);
                shader.SetParam(2, color);

                GL.DrawElements(BeginMode.Triangles, Mesh.quad.indices.Length, DrawElementsType.UnsignedInt, Mesh.quad.ebo);
            }
        }
    }
}
