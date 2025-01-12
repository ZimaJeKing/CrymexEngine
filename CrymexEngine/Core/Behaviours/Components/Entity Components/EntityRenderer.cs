using CrymexEngine.Rendering;
using CrymexEngine.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public sealed class EntityRenderer : EntityComponent
    {
        public float Depth
        {
            get
            {
                return _depth;
            }
            set
            {
                value = Math.Clamp(value, -99f, 99f);

                _depth = value;
            }
        }

        public Texture texture = Texture.None;
        public Color4 color = Color4.White;
        public Mesh mesh = Mesh.quad;
        public Shader shader = Shader.Regular;

        private float _depth;

        protected override void Update()
        {
            if (Vector2.DistanceSquared(entity.Position, Camera.position) < Camera.renderDistanceSquared)
            {
                GL.UseProgram(shader._glShader);

                // Bind buffers
                GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.vbo);
                GL.BindVertexArray(mesh.vao);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.ebo);

                // Set first three shader parameters for Position, transformation, and color
                shader.SetParam(0, VectorUtility.Vec2ToVec3((entity.Position - Camera.position) / Window.HalfSize, -Depth * 0.001f));
                shader.SetParam(1, entity.TransformationMatrix);
                shader.SetParam(2, color);

                GameObject.GameObjectPreRender(entity);

                GL.DrawElements(BeginMode.Triangles, mesh.indices.Length, DrawElementsType.UnsignedInt, mesh.ebo);
            }
        }
    }
}