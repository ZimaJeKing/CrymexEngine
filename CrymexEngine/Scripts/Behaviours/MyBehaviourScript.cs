using CrymexEngine.Rendering;
using CrymexEngine.UI;
using CrymexEngine.Utils;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts
{
    public class MyBehaviourScript : ScriptableBehaviour
    {
        protected override void Load()
        {
            Debug.Log(Assets.GetText("MyText"));
        }

        protected override void Update()
        {
            
        }
    }
}