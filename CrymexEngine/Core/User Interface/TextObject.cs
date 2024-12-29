using CrymexEngine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CrymexEngine.UI
{
    public class TextObject
    {
        public bool enabled = true;

        public Vector2 position;

        public Vector2i Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _halfScale = value;
                _scale = value;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text == value) return;
                _text = value;
                ReDraw();
            }
        }

        public Color4 FontColor
        {
            get
            {
                return _fontColor;
            }
            set
            {
                if (_fontColor == value) return;
                _fontColor = value;
                ReDraw();
            }
        }

        public Color4 BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                if (_backgroundColor == value) return;
                _backgroundColor = value;
                ReDraw();
            }
        }

        public Alignment Alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                _alignment = value;
                ReDraw();
            }
        }

        public Vector2 TextPadding
        {
            get
            {
                return _textPadding;
            }
            set
            {
                _textPadding = value;
                ReDraw();
            }
        }

        public FontFamily Family
        {
            get
            {
                return _family;
            }
            set
            {
                _family = value;
                ReDraw();
            }
        }
        public FontStyle Style
        {
            get
            {
                return _style;
            }
            set
            {
                _style = value;
                ReDraw();
            }
        }
        public float FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                _fontSize = value;
                ReDraw();
            }
        }

        public Vector2 PhysicalTextSize => _physicalTextSize;
        public Texture InternalTexture => _texture;
        public Vector2 HalfScale => _halfScale;

        private string _text;
        private FontFamily _family;
        private FontStyle _style;
        private float _fontSize;
        private Color4 _fontColor = Color4.Black;
        private Color4 _backgroundColor = Color4.Transparent;
        private Texture _texture;
        private Matrix4 _scaleMatrix;
        private Alignment _alignment;
        private Vector2i _scale;
        private Vector2 _halfScale;
        private Vector2 _textPadding;
            
        private Vector2 _physicalTextSize;

        public TextObject(Vector2 position, Vector2i scale, string text, FontFamily family, float fontSize, Alignment textAlignment = Alignment.MiddleCenter, FontStyle style = FontStyle.Regular)
        {
            this.position = position;
            _scale = scale;
            _halfScale = scale.ToVector2() * 0.5f;
            _text = text;
            _family = family;
            _style = style;
            _fontSize = fontSize;
            _alignment = textAlignment;

            _physicalTextSize = Measure(Family.CreateFont(_fontSize, _style));

            Scenes.Scene.current.textObjects.Add(this);

            ReDraw();
        }

        public void Render()
        {
            if (_family == default) return;

            Shader.UI.Use();

            // Bind shader and texture
            GL.BindTexture(TextureTarget.Texture2D, _texture.glTexture);
            GL.BindVertexArray(Mesh.quad.vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Mesh.quad.ebo);

            // Set first three shader parameters for Position, transformation, and color
            Shader.UI.SetParam(0, Debug.Vec2ToVec3(position / Window.HalfSize, 0));
            Shader.UI.SetParam(1, _scaleMatrix);
            Shader.UI.SetParam(2, Color4.White);

            GL.DrawElements(BeginMode.Triangles, Mesh.quad.indices.Length, DrawElementsType.UnsignedInt, Mesh.quad.ebo);
        }

        private void ReDraw()
        {
            if (_family == default) return;

            Vector2 alignmentOffset = GetAlignmentOffset(_alignment);
            System.Drawing.Color sysBackColor = System.Drawing.Color.FromArgb(_backgroundColor.ToArgb());
            System.Drawing.Color sysFontColor = System.Drawing.Color.FromArgb(_fontColor.ToArgb());

            _texture?.Dispose();

            // Measure the text and calculate hotspot
            Font font = Family.CreateFont(_fontSize, _style);
            _physicalTextSize = Measure(font);

            alignmentOffset.X -= _physicalTextSize.X / 2f;
            alignmentOffset.Y -= _physicalTextSize.Y / 2f;

            using (Image<Rgba32> image = new Image<Rgba32>(_scale.X, _scale.Y))
            {
                // Render the text
                image.Mutate(ctx =>
                {
                    ctx.Fill(Color.FromRgba(sysBackColor.R, sysBackColor.G, sysBackColor.B, sysBackColor.A));
                    ctx.DrawText(_text, font, Color.FromRgba(sysFontColor.R, sysFontColor.G, sysFontColor.B, sysFontColor.A), new PointF(alignmentOffset.X + _textPadding.X, alignmentOffset.Y + _textPadding.Y));
                });

                // Apply to a texture
                _texture = Texture.FromImageSharp(image);
            }

            _texture.FlipY();
            _scaleMatrix = Matrix4.CreateScale(Debug.Vec2ToVec3(_scale / Window.HalfSize, 1));
        }

        private Vector2 GetAlignmentOffset(Alignment alignment)
        {
            // Precomputed scale offsets
            float fullWidth = _scale.X;
            float fullHeight = _scale.Y;

            return alignment switch
            {
                Alignment.TopLeft => new Vector2(0, 0),
                Alignment.TopCenter => new Vector2(fullWidth / 2f, 0),
                Alignment.TopRight => new Vector2(fullWidth, 0),
                Alignment.MiddleLeft => new Vector2(0, fullHeight / 2f),
                Alignment.MiddleCenter => new Vector2(fullWidth / 2f, fullHeight / 2f),
                Alignment.MiddleRight => new Vector2(fullWidth, fullHeight / 2f),
                Alignment.BottomLeft => new Vector2(0, fullHeight),
                Alignment.BottomCenter => new Vector2(fullWidth / 2f, fullHeight),
                Alignment.BottomRight => new Vector2(fullWidth, fullHeight),
                _ => Vector2.Zero // Default case
            };
        }

        private Vector2 Measure(Font font)
        {
            var textOptions = new TextOptions(font)
            {
                Origin = PointF.Empty
            };
            FontRectangle rect = TextMeasurer.MeasureSize(_text, textOptions);
            return new Vector2(rect.Width, rect.Height);
        }
    }

    public enum Alignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}
