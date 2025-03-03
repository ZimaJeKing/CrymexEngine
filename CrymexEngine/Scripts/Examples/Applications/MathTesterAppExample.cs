using CrymexEngine.UI;
using CrymexEngine.Utils;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class MathTesterAppExample : ScriptableBehaviour
    {
        private MathExpression _mathExpression;

        private TextObject _mathExpressionText;
        private TextObject _correctnessText;
        private TextObject _progressText;
        private InputField _mainInputField;

        private int _questionNumber = 1;
        private int _correctAnswers = 0;

        private const int questionsTotal = 50;

        protected override void Load()
        {
            CreateHeaderText();

            CreateMathExpressionText();

            CreateMainInputField();

            CreateConfirmButton();

            CreateStatusTexts();

            _mathExpression = GenerateExpression();
            DisplayExpression(_mathExpression);
        }

        protected override void Update() { }

        private void CreateStatusTexts()
        {
            _correctnessText = new TextObject(new Vector2(-Window.HalfSize.X + 200, 256), new Vector2i(400, 30), "Correct answers: 0 / 0", Assets.DefaultFontFamily, 22, Alignment.MiddleLeft);
            _correctnessText.FontColor = Color4.White;
            _correctnessText.TextPadding = new Vector2(10, 0);

            _progressText = new TextObject(new Vector2(-Window.HalfSize.X + 200, 220), new Vector2i(400, 30), $"Question: 1 / {questionsTotal}", Assets.DefaultFontFamily, 22, Alignment.MiddleLeft);
            _progressText.FontColor = Color4.White;
            _progressText.TextPadding = new Vector2(10, 0);
        }

        private void CreateConfirmButton()
        {
            Button confirmButton = new UIElement(Texture.White, new Vector2(0, -80), new Vector2(128, 64)).AddComponent<Button>();
            confirmButton.onClick += ConfirmButtonClick;
            TextObject confirmButtonText = new TextObject(new Vector2(0, -80), new Vector2i(128, 64), "Confirm", Assets.DefaultFontFamily, 25);
        }
        private void CreateMathExpressionText()
        {
            _mathExpressionText = new TextObject(new Vector2(0, 128), new Vector2i(300, 32), "1 + 1 =", Assets.DefaultFontFamily, 30);
            _mathExpressionText.FontColor = Color4.White;
        }
        private void CreateHeaderText()
        {
            TextObject text = new TextObject(new Vector2(0, 300), new Vector2i(720, 50), "Math training application", Assets.GetFontFamily("Jakarta"), 30);
            text.BackgroundColor = Color4.White;
            text.FontColor = Color4.Red;
        }
        private void CreateMainInputField()
        {
            UIElement inputFieldElement = new UIElement(Texture.White, Vector2.Zero, new Vector2(512, 64), null, "InputField", 0);
            _mainInputField = inputFieldElement.AddComponent<InputField>();
            _mainInputField.CharacterLimit = 30;
            _mainInputField.DisplayText.FontSize = 32;
            _mainInputField.onSubmit += ConfirmInput;
        }

        private void ConfirmInput(string input)
        {
            if (int.TryParse(input, out int userResult))
            {
                bool isCorrect = userResult == _mathExpression.result;
                SpawnAnswerBox(isCorrect);

                _mathExpression = GenerateExpression();
                DisplayExpression(_mathExpression);

                _questionNumber++;
                if (isCorrect) _correctAnswers++;

                if (_questionNumber > questionsTotal)
                {
                    _questionNumber = 1;
                    _correctAnswers = 0;
                }

                _correctnessText.Text = $"Correct answers: {_correctAnswers} / {_questionNumber - 1}";
                _progressText.Text = $"Question: {_questionNumber} / {questionsTotal}";
            }
            _mainInputField.Value = string.Empty;
        }

        private void ConfirmButtonClick(MouseButton button)
        {
            ConfirmInput(_mainInputField.Value);
        }

        private void DisplayExpression(MathExpression expression)
        {
            _mathExpressionText.Text = $"{expression.left} {expression.usedOperator} {expression.right} =";
        }

        private MathExpression GenerateExpression()
        {
            int leftValue;
            int rightValue;
            int result;

            char usedOperator = MathExpression.supportedOperators[Random.Range(1, MathExpression.supportedOperators.Length)];

            switch (usedOperator)
            {
                case '+':
                    {
                        leftValue = Random.Range(0, 100);
                        rightValue = Random.Range(0, 100);
                        result = leftValue + rightValue;
                        break;
                    }
                case '-':
                    {
                        leftValue = Random.Range(0, 100);
                        rightValue = Random.Range(1, leftValue);
                        result = leftValue - rightValue;
                        break;
                    }
                case '*':
                    {
                        leftValue = Random.Range(1, 11);
                        rightValue = Random.Range(1, 11);
                        result = leftValue * rightValue;
                        break;
                    }
                case '/':
                    {
                        leftValue = Random.Range(0, 100);
                        rightValue = Random.Range(2, 10);

                        leftValue = (leftValue / rightValue) * rightValue;
                        result = leftValue / rightValue;
                        break;
                    }
                default:
                    {
                        leftValue = 0;
                        rightValue = 0;
                        result = 0;
                        break;
                    }
            }

            return new MathExpression(leftValue, rightValue, usedOperator, result);
        }

        private void SpawnAnswerBox(bool correct)
        {
            UIElement answerBox = new UIElement(Texture.White, new Vector2(0, -256), new Vector2(256, 64));
            answerBox.Renderer.color = new Color4(1f, 1f, 1f, 0.5f);
            AnswerBoxComponent answerBoxComponent = answerBox.AddComponent<AnswerBoxComponent>();

            if (correct)
            {
                answerBoxComponent.SetText("Correct answer");
            }
            else
            {
                answerBoxComponent.SetText("Wrong answer");
            }
        }
    }

    /// <summary>
    /// Component for displaying a text message for a short period of time
    /// </summary>
    class AnswerBoxComponent : UIComponent
    {
        private float startTime;
        private float showTime = 1; // 1 seconds

        private TextObject text;

        public override void PreRender()
        {
        }

        protected override void Load()
        {
            text = new TextObject(Element.Position, VectorUtil.RoundToInt(Element.Scale), string.Empty, Assets.DefaultFontFamily, 25);
            startTime = Time.GameTime;
        }

        public void SetText(string displayText)
        {
            text.Text = displayText;
        }

        protected override void Update()
        {
            if (Time.GameTime - startTime > showTime)
            {
                text.Delete();
                Element.Delete();
            }
        }
    }

    class MathExpression
    {
        public static readonly char[] supportedOperators = { '+', '-', '*', '/' };

        public readonly int left;
        public readonly int right;
        public readonly int result;
        public readonly char usedOperator;

        public MathExpression(int left, int right, char usedOperator, int result)
        {
            this.left = left;
            this.right = right;
            this.usedOperator = usedOperator;
            this.result = result;
        }
    }
}
