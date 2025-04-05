namespace CrymexEngine
{
    public abstract class Behaviour
    {
        public bool enabled = true;

        public Behaviour()
        {
            if (!Window.Loaded) throw new Exception("Trying to instantiate a behaviour before Running the engine is strictly prohibited");
        }

        protected Settings GlobalSettings => Settings.GlobalSettings;
        protected Texture GetTex(string name) => Assets.GetTexture(name);
        protected AudioClip? GetAud(string name) => Assets.GetAudioClip(name);

        protected abstract void Load();
        protected abstract void Update();
        protected abstract void OnQuit();

        internal static void LoadBehaviour(Behaviour behaviour)
        {
            if (behaviour == null) return;

            behaviour.Load();
        }
        internal static void UpdateBehaviour(Behaviour behaviour)
        {
            if (behaviour == null) return;

            behaviour.Update();
        }
        internal static void QuitBehaviour(Behaviour behaviour)
        {
            if (behaviour == null) return;

            behaviour.OnQuit();
        }
    }
}
