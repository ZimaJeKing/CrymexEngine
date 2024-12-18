﻿using CrymexEngine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;
using System.Drawing;

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

        public override void Update()
        {
            if (Vector2.DistanceSquared(Entity.Position, Camera.position) < Camera.renderDistanceSquared)
            {
                GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.vbo);
                GL.BindVertexArray(mesh.vao);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.ebo);

                GL.UseProgram(shader._glShader);

                // Set first three shader parameters for Position, transformation, and color
                Vector2 glPosition2D = (Entity.Position - Camera.position) / Window.HalfSize;
                Vector3 glPosition3D = new Vector3(glPosition2D.X, glPosition2D.Y, -Depth * 0.01f);

                shader.SetParam(0, glPosition3D);
                shader.SetParam(1, Entity.transformationMatrix);
                shader.SetParam(2, color);

                Entity.PreRender();

                GL.DrawElements(BeginMode.Triangles, mesh.indices.Length, DrawElementsType.UnsignedInt, mesh.ebo);
            }
        }
    }
}