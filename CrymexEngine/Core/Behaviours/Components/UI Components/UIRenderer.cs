using CrymexEngine.Rendering;
using CrymexEngine.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    [FreeComponent]
    public class UIRenderer : UIComponent
    {
        public Texture texture = Texture.White;
        public Shader shader = Shader.UI;
        public Color4 color = Color4.White;
        public Vector2 UVTiling
        {
            get
            {
                return _uvTiling;
            }
            set
            {
                _uvTiling = value;
                _uvTransform = new Vector4(_uvTiling.X, _uvTiling.Y, _uvOffset.X, _uvOffset.Y);
            }
        }
        public Vector2 UVOffset
        {
            get
            {
                return _uvOffset;
            }
            set
            {
                _uvOffset = value;
                _uvTransform = new Vector4(_uvTiling.X, _uvTiling.Y, _uvOffset.X, _uvOffset.Y);
            }
        }

        public float Depth
        {
            get
            {
                return _depth;
            }
            set
            {
                _depth = value;
                UICanvas.Instance.SortElements();
            }
        }

        private float _depth = 0;
        private Vector2 _uvTiling;
        private Vector2 _uvOffset;
        private Vector4 _uvTransform = new Vector4(1, 1, 0, 0);

        public UIRenderer(float depth)
        {
            _depth = depth;
            UICanvas.Instance.SortElements();
        }

        protected override void Update()
        {
            // Shaders not found
            if (shader == null)
            {
                Debug.LogError($"Shaders not found for object '{uiElement.name}'");
                return;
            }

            shader.Use();

            // Bind shader and texture
            GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);
            GL.BindBuffer(BufferTarget.ArrayBuffer, Mesh.quad.vbo);
            GL.BindVertexArray(Mesh.quad.vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Mesh.quad.ebo);

            // Set first three shader parameters for Position, transformation, and color
            shader.SetParam(0, VectorUtility.Vec2ToVec3(uiElement.Position / Window.HalfSize, 0));
            shader.SetParam(1, uiElement.TransformationMatrix);
            shader.SetParam(2, color);
            shader.SetParam(3, _uvTransform);

            GL.DrawElements(BeginMode.Triangles, Mesh.quad.indices.Length, DrawElementsType.UnsignedInt, Mesh.quad.ebo);
        }

        public override void PreRender()
        {
        }

        protected override void Load()
        {
        }
    }
}
