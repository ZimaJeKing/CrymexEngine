using CrymexEngine.GameObjects;
using CrymexEngine.Rendering;
using CrymexEngine.UI;
using CrymexEngine.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    [SingularComponent]
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
            // Shaders not found
            if (shader == null)
            {
                enabled = false;
                Debug.LogError($"Shaders not found for object '{entity.name}'");
                return;
            }

            if (Vector2.DistanceSquared(entity.Transform.Position, Camera.position) < Camera.renderDistanceSquared)
            {
                shader.Use();

                // Bind buffers
                GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);
                GL.BindVertexArray(mesh.vao);

                // Set first three shader parameters for Position, transformation, and color
                shader.SetParam(0, VectorUtil.Vec2ToVec3((entity.Transform.Position - Camera.position) / Window.HalfSize, -Depth * 0.001f));
                shader.SetParam(1, entity.Transform.TransformationMatrix);
                shader.SetParam(2, color);

                GameObject.GameObjectPreRender(entity);

                GL.DrawElements(PrimitiveType.Triangles, mesh.indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.UseProgram(0);
            }
        }

        public override void PreRender() { }

        protected override void Load() { }
    }
}