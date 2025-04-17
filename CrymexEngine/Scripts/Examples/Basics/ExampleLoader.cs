using CrymexEngine.Scripting;

namespace CrymexEngine.Scripts.Examples
{
    public class ExampleLoader : ScriptableBehaviour
    {
        public Type[] exampleBehaviours =
        {
            typeof(TransformExample),
            typeof(GradientExample),
            typeof(LineExample),
            typeof(LineGroupExample),
            typeof(TextExample),
            typeof(InputFieldExample),
            typeof(UVTransformExample)
        };

        private int _exampleIndex = 0;

        private bool shouldSwitch = false;

        protected override void Load()
        {
            stayAlive = true;
            ScriptLoader.RuntimeAdd(exampleBehaviours[_exampleIndex]);
        }

        protected override void Update()
        {
            if (shouldSwitch)
            {
                shouldSwitch = false;
                ScriptLoader.RuntimeAdd(exampleBehaviours[_exampleIndex]);
            }

            if (Input.KeyDown(Key.Right))
            {
                _exampleIndex++;
                _exampleIndex %= exampleBehaviours.Length;
                shouldSwitch = true;
                Scenes.Scene.Current.Clear();
            }
            else if (Input.KeyDown(Key.Left))
            {
                _exampleIndex--;
                if (_exampleIndex < 0) _exampleIndex = exampleBehaviours.Length - 1;
                shouldSwitch = true;
                Scenes.Scene.Current.Clear();
            }
        }
    }
}
