﻿using CrymexEngine.Rendering;
using CrymexEngine.UI;
using CrymexEngine.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.UI
{
    public class TextEditor
    {
        public static TextEditor Instance => _instance;

        private static InputField? _selected;
        private static int _selectedIndex;
        private static float _lastEditTime;

        private static Matrix4 _cursorTransform;
        private static Vector2 _cursorPosition;

        private static readonly TextEditor _instance = new TextEditor();

        public static void Select(InputField? inputField, int index)
        {
            if (_selected == inputField)
            {
                _selected = null;
                return;
            }

            _selected = inputField;
            _selectedIndex = index;

            RecalcCursor(_selected.Value);
        }

        public static void Deselect()
        {
            _selected = null;
        }

        public void Update()
        {
            if (_selected == null) return;

            if (Input.KeyDown(Key.Enter))
            {
                _selected.onSubmit?.Invoke(_selected.Value);
                _selected = null;
                return;
            }

            string newVal = Write(_selected.Value, out bool edited);
            if (edited)
            {
                RecalcCursor(newVal);
                _selected.Value = newVal;
            }
        }

        private static string Write(string input, out bool edited)
        {
            edited = false;
            string final = "";

            if (input.Length == 0) // First input
            {
                if (Input.textInput.Length != 0) // Text input
                {
                    final = Input.textInput;
                    edited = true;
                    _selectedIndex = final.Length - 1;
                }
                _selected.onTextInput?.Invoke(final);
                return final;
            }

            int arrowMovement = (int)Axis.ArrowsLR.GetValue();
            float editTimeDif = Time.GameTime - _lastEditTime;

            string firstPart = input.Substring(0, _selectedIndex + 1);
            string secondPart = "";
            if (input.Length >= _selectedIndex + 1) secondPart = input.Substring(_selectedIndex + 1);

            if (Input.Key(Key.Backspace) && editTimeDif > 0.1f && _selectedIndex >= 0) // Erasing
            {
                _lastEditTime = Time.GameTime;

                final = firstPart[0..^1] + secondPart;
                _selectedIndex--;
                edited = true;
                _selected.onTextInput?.Invoke("\b");
            }
            else if (arrowMovement != 0 && editTimeDif > 0.15f) // Arrow movement
            {
                _lastEditTime = Time.GameTime;
                _selectedIndex += arrowMovement;
                _selectedIndex = Math.Clamp(_selectedIndex, -1, input.Length - 1);
            }
            else if (Input.textInput.Length < _selected.CharacterLimit) // Text input
            {
                final = firstPart + Input.textInput + secondPart;
                _selectedIndex += Input.textInput.Length;
                edited = true;
                _selected.onTextInput?.Invoke(Input.textInput);
            }

            return final;
        }

        public void RenderCursor()
        {
            if (_selected == null) return;

            if (((int)(Time.GameTime * 2)) % 2 == 0) return; // Cursor blinking in half second interval

            Shader.Regular.Use();

            // Bind buffers
            GL.BindTexture(TextureTarget.Texture2D, Texture.White.glTexture);
            GL.BindVertexArray(Mesh.quad.vao);

            // Set first three shader parameters for Position, transformation, and color
            Shader.Regular.SetParam(0, VectorUtil.Vec2ToVec3(_cursorPosition / Window.HalfSize, 0));
            Shader.Regular.SetParam(1, _cursorTransform);
            Shader.Regular.SetParam(2, Color4.Black);

            GL.DrawElements(PrimitiveType.Triangles, Mesh.quad.indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public static void RecalcCursor(string newText)
        {
            _cursorTransform = Matrix4.CreateScale(VectorUtil.Vec2ToVec3(new Vector2(Math.Max(2, _selected.DisplayText.FontSize * 0.05f), _selected.DisplayText.FontSize) / Window.HalfSize, 0));

            if (newText.Length == 0 || _selectedIndex == -1)
            {
                _cursorPosition = _selected.Element.Transform.Position - new Vector2(_selected.Element.Transform.HalfScale.X - _selected.DisplayText.TextPadding.X, 0);
                return;
            }

            string firstTextPart = newText.Substring(0, _selectedIndex + 1);
            _cursorPosition = _selected.Element.Transform.Position + new Vector2(-_selected.Element.Transform.HalfScale.X + _selected.DisplayText.Measure(firstTextPart).X + _selected.DisplayText.TextPadding.X + 5, 0);
        }
    }
}
