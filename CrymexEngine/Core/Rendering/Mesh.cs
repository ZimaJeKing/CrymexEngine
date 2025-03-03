using OpenTK.Graphics.OpenGL;

namespace CrymexEngine
{
    public class Mesh
    {
        /// <summary>
        /// Order x, y, uvX, uvY
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

            // aPosition
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // aTexCoord
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            ebo = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public static readonly Mesh quad = new(
            new float[] {
             -0.5f, -0.5f, 0.0f, 0.0f,
              0.5f, -0.5f, 1.0f, 0.0f,
              0.5f,  0.5f, 1.0f, 1.0f,
             -0.5f,  0.5f, 0.0f, 1.0f
            },
            new int[] { 0, 1, 2, 2, 3, 0 }
            );

        public static readonly Mesh line = new(
            new float[]
            {
                0.0f, -0.5f, 0.0f, 0.0f,
                0.0f,  0.5f, 1.0f, 1.0f

            },
            new int[] { 0, 1 }
            );
    }
}