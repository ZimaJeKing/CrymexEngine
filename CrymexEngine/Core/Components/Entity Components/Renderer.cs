using CrymexEngine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;

namespace CrymexEngine
{
    public class Renderer : EntityComponent
    {
        public Texture texture = Texture.None;
        public Color4 color = Color4.White;
        public float depth;
        public Mesh mesh = Mesh.quad;
        public Shader shader = Shader.regular;

        public override void Update()
        {
            if (Vector2.DistanceSquared(Entity.position, Camera.position) < Camera.renderDistanceSquared)
            {
                // Bind texture
                GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);
                GL.BindVertexArray(mesh.vao);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.ebo);
                GL.UseProgram(shader._glShader);

                // Set first three shader parameters for position, transformation, and color
                Vector2 glPosition2D = (Entity.position - Camera.position) / (Window.Size.ToVector2() * 0.5f);
                Vector3 glPosition3D = new Vector3(glPosition2D.X, glPosition2D.Y, depth);

                shader.SetParam(0, glPosition3D);
                shader.SetParam(1, Entity.scaleMatrix * Entity.rotationMatrix);
                shader.SetParam(2, color);

                Entity.PreRender();

                GL.DrawElements(BeginMode.Triangles, mesh.indices.Length, DrawElementsType.UnsignedInt, mesh.ebo);
            }
        }
    }

    public class Mesh
    {
        /// <summary>
        /// Order x, y, z
        /// </summary>
        public float[] vertices;
        public int[] indices;
        public int vbo;
        public int vao;
        public int ebo;

        public Mesh(float[] vertices, int[] indices)
        {
            this.indices = indices;
            this.vertices = vertices;

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            ebo = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.StaticDraw);

            GL.ActiveTexture(TextureUnit.Texture0);
        }

        public static Mesh quad = new Mesh(
            new float[] {
            0.5f, 0.5f, 0.0f,
              0.5f, -0.5f, 0.0f,
             -0.5f, -0.5f, 0.0f,
             -0.5f, 0.5f, 0.0f
            },
            new int[] { 0, 1, 3, 1, 2, 3 }
            );
    }
}