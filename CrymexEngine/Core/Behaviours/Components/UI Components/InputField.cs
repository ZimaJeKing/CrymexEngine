using CrymexEngine.Core.UI;
using CrymexEngine.UI;
using CrymexEngine.Utils;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrymexEngine.UI
{
    public class InputField : UIComponent
    {
        public Action<string> onSubmit;
        public Action<string> onTextInput;

        public Button ButtonComponent => _buttonComponent;
        public TextObject DisplayText => _displayText;

        public bool changeCursor = true;

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value == null)
                {
                    _value = "";
                    _displayText.Text = "";
                }
                else
                {
                    value = value.Replace("\n", "");
                    _value = value;
                    _displayText.Text = value;
                }
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
        private string _value = "";
        private int _characterLimit = int.MaxValue;

        public override void Load()
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
        }

        public override void OnMouseEnter()
        {
            if (changeCursor) Window.Instance.GLFWWindow.Cursor = MouseCursor.IBeam;
        }

        public override void OnMouseExit()
        {
            if (changeCursor) Window.Cursor = Window.Cursor;
        }

        private void Select(MouseButton button)
        {
            TextEditor.Select(this, Math.Max(_value.Length - 1, 0));
        }
    }
}
