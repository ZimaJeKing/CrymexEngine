using CrymexEngine.Audio;
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
        }

        protected override void Update()
        {
            if (Input.KeyDown(Key.F))
            {
                ALMgr.Play(GetSound("SprayPaint"), 0.25f, false);
            }
        }
    }
}