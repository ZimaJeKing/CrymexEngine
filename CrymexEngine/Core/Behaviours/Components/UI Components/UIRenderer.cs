﻿using CrymexEngine.Rendering;
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
                UICanvas.Instance.SortElements();
            }
        }

        private float _depth = 0;

        public UIRenderer(float depth)
        {
            _depth = depth;
            UICanvas.Instance.SortElements();
        }

        public override void Update()
        {
            shader.Use();

            // Bind shader and texture
            GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);
            GL.BindBuffer(BufferTarget.ArrayBuffer, Mesh.quad.vbo);
            GL.BindVertexArray(Mesh.quad.vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Mesh.quad.ebo);

            // Set first three shader parameters for Position, transformation, and color
            shader.SetParam(0, Debug.Vec2ToVec3(UIElement.Position / Window.Size, 0));
            shader.SetParam(1, UIElement.TransformationMatrix);
            shader.SetParam(2, color);

            GL.DrawElements(BeginMode.Triangles, Mesh.quad.indices.Length, DrawElementsType.UnsignedInt, Mesh.quad.ebo);
        }
    }
}
