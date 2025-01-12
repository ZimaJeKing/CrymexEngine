using CrymexEngine.Utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;

namespace CrymexEngine.UI
{
    public class InputField : UIComponent, IMouseClick, IMouseHover
    {
        /// <summary>
        /// Happens if the user presses the enter key with the InputField selected
        /// </summary>
        public Action<string> onSubmit;
        public Action<string> onTextInput;

        public Button ButtonComponent => _buttonComponent;
        public TextObject DisplayText => _displayText;

        public bool changeCursor = true;

        public string PreviewText
        {
            get
            {
                return _previewText;
            }
            set 
            { 
                _previewText = value;
                ShowPreviewText();
            }
        }

        public Color4 PreviewTextColor
        {
            get
            {
                return _previewTextColor;
            }
            set
            {
                _previewTextColor = value;
            }
        }
        public Color4 TextColor
        {
            get
            {
                return _textColor;
            }
            set
            {
                _textColor = value;
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!_selected) ShowPreviewText();
                    _value = string.Empty;
                    _displayText.Text = string.Empty;
                    return;
                }

                // Make text one line
                value = value.Replace("\n", string.Empty);

                // If value is an empty string display preview
                if (string.IsNullOrEmpty(value) && !_selected)
                {
                    _displayText.FontColor = _previewTextColor;
                    _displayText.Text = _previewText;
                }

                _value = value;
                _displayText.FontColor = _textColor;
                _displayText.Text = value;
            }
        }
        public int CharacterLimit
        {
            get
            {
                return _characterLimit;
            }
            set
            {
                if (value < 0) return;
                _characterLimit = value;
            }
        }

        private Button _buttonComponent;
        private TextObject _displayText;
        private string _value = string.Empty;
        private int _characterLimit = int.MaxValue;
        private string _previewText = string.Empty;
        private Color4 _previewTextColor = new Color4(0, 0, 0, 128);
        private Color4 _textColor = Color4.Black;
        private bool _selected = false;

        protected override void Load()
        {
            _displayText = new TextObject(UIElement.Position, VectorUtility.RoundToInt(UIElement.Scale), "", Assets.DefaultFontFamily, (int)(UIElement.Scale.Y * 0.8f), Alignment.MiddleLeft);
            _displayText.BestFit = true;
            _displayText.TextPadding = new Vector2(10, 0);
            _displayText.MaxBestFitSize = (int)(UIElement.Scale.Y * 0.8f);

            _buttonComponent = UIElement.GetComponent<Button>();
            if (_buttonComponent == null)
            {
                _buttonComponent = UIElement.AddComponent<Button>();
            }
            _buttonComponent.onClick = Select;

            ShowPreviewText();
        }

        public void OnMouseEnter()
        {
            if (changeCursor) Window.Instance.GLFWWindow.Cursor = MouseCursor.IBeam;
        }

        public void OnMouseExit()
        {
            if (changeCursor) Window.Cursor = Window.Cursor;
        }

        private void Select(MouseButton button)
        {
            _selected = !_selected;

            if (_selected)
            {
                if (string.IsNullOrEmpty(_value))
                {
                    _displayText.Text = string.Empty;
                    _displayText.FontColor = _textColor;
                }
            }
            else
            {
                ShowPreviewText();
            }
            TextEditor.Select(this, Math.Max(_value.Length - 1, 0));
        }

        private void ShowPreviewText()
        {
            if (string.IsNullOrEmpty(_value))
            {
                _displayText.Text = _previewText;
                _displayText.FontColor = _previewTextColor;
            }
        }

        public override void PreRender()
        {
        }

        protected override void Update()
        {
        }

        public void OnMouseDown(MouseButton mouseButton)
        {
        }

        public void OnMouseHold(MouseButton mouseButton, float time)
        {
        }

        public void OnMouseUp()
        {
        }

        public void OnMouseStay(float time)
        {
        }
    }
}
