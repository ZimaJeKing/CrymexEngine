using CrymexEngine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public class UIRenderer : UIComponent
    {
        public Texture texture;
        public Shader shader = Shader.UI;
        public Color4 color = Color4.White;

        public float Depth
        {
            get
            {
                return _depth;
            }
            set
            {
                _depth = value;
                UICanvas.SortElements();
            }
        }

        private float _depth = 0;

        public UIRenderer(float depth)
        {
            _depth = depth;
            UICanvas.SortElements();
        }

        public override void Update()
        {
            // Bind shader and texture
            GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);
            GL.BindVertexArray(Mesh.quad.vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Mesh.quad.ebo);
            GL.UseProgram(shader._glShader);

            // Set first three shader parameters for Position, transformation, and color
            Vector2 glPosition2D = UIElement.Position / Window.HalfSize;
            Vector3 glPosition3D = new Vector3(glPosition2D.X, glPosition2D.Y, -Depth * 0.01f);

            shader.SetParam(0, glPosition3D);
            shader.SetParam(1, UIElement.transformationMatrix);
            shader.SetParam(2, color);

            GL.DrawElements(BeginMode.Triangles, Mesh.quad.indices.Length, DrawElementsType.UnsignedInt, Mesh.quad.ebo);
        }
    }
}
