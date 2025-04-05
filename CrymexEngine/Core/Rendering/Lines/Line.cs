using CrymexEngine.Rendering;
using CrymexEngine.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class Line : Behaviour
    {
        public readonly LineGroup group;

        public Shader shader = Shader.Line;
        public Color4 color = Color4.White;

        private Vector2 _screenPosition;
        private bool _screenSpace = false;
        private Vector2 _start;
        private Vector2 _end;
        private Matrix4 _transform;
        private float _width;
        private float _depth;
        private Vector2 _middlePoint;
        private Vector2 _pointDif;

        public Vector2 MiddlePoint => _middlePoint;
        public float Depth
        {
            get => _depth;
            set => _depth = -Math.Clamp(value, 0.001f, 1f);
        }
        public float Width
        {
            get => _width;
            set => _width = Math.Clamp(value, 0.1f, 100f);
        }
        public bool ScreenSpace
        {
            get => _screenSpace;
            set
            {
                _screenSpace = value;
                RecalcTransform();
            }
        }
        public Vector2 Start
        {
            get => _start;
            set
            {
                _start = value;
                RecalcTransform();            
            }
        }
        public Vector2 End
        {
            get => _end;
            set
            {
                _end = value;
                RecalcTransform();
            }
        }

        /// <summary>
        /// When assigning both points at the same time use this property, as it is faster
        /// </summary>
        public (Vector2, Vector2) StartEnd
        {
            get => (_start,  _end);
            set
            {
                _start = value.Item1;
                _end = value.Item2;
                RecalcTransform();
            }
        }

        public Line(Vector2 start, Vector2 end, Color4 color, float width = 1, float depth = 0, bool screenSpace = false)
        {
            _start = start;
            _end = end;
            this.color = color;
            Width = width;
            Depth = depth;
            _screenSpace = screenSpace;

            RecalcTransform();

            group = new LineGroup(this);
            Scenes.Scene.Current.lines.Add(group);
        }
        internal Line(Vector2 start, Vector2 end, Color4 color, LineGroup group, float width = 1, float depth = 0, bool screenSpace = false)
        {
            this.group = group;

            _start = start;
            _end = end;
            this.color = color;
            Width = width;
            Depth = depth;
            _screenSpace = screenSpace;

            RecalcTransform();

        }

        public Vector2 GetPointOnLine(float x)
        {
            x = Math.Clamp(x, 0f, 1f);

            return _start + (_pointDif * x);
        }

        private void RecalcTransform()
        {
            _pointDif = _end - _start;

            float rotation = MathF.Atan(_pointDif.Y / _pointDif.X);

            // If the points have the same X coordinate, rotation will be NaN
            if (float.IsNaN(rotation)) rotation = 0;

            _middlePoint = GetPointOnLine(0.5f);

            if (_screenSpace)
            {
                _screenPosition = _middlePoint * Window.OneOverHalfSize;
            }
            else
            {
                _screenPosition = (_middlePoint - Camera.position) * Window.OneOverHalfSize;
            }

            _transform = Matrix4.CreateRotationZ(-rotation - (MathF.PI * 0.5f))
                       * Matrix4.CreateScale(1, _pointDif.LengthFast * Window.OneOverHalfSize.Y, 1);
        }

        public void Delete()
        {
            group.Delete();
        }

        protected override void Load()
        {
        }

        protected override void Update()
        {
            if (shader == null) return;

            // Bind buffers
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, Mesh.line.vbo);
            GL.BindVertexArray(Mesh.line.vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Mesh.line.ebo);

            shader.Use();

            shader.SetParam(0, VectorUtil.Vec2ToVec3(_screenPosition, _depth));

            shader.SetParam(1, _transform);
            shader.SetParam(2, color);

            GL.LineWidth(_width);

            GL.DrawElements(BeginMode.Lines, Mesh.line.indices.Length, DrawElementsType.UnsignedInt, Mesh.line.ebo);

            GL.UseProgram(0);
        }

        protected override void OnQuit() { }
    }
}
