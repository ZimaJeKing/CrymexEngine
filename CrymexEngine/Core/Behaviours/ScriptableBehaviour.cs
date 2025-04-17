namespace CrymexEngine
{
    public abstract class ScriptableBehaviour : Behaviour
    {
        public bool stayAlive = false;

        protected override void OnQuit()
        {
        }
    }
}
