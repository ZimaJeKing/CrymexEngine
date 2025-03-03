namespace CrymexEngine
{
    public abstract class Behaviour
    {
        public bool enabled = true;

        public Behaviour()
        {
            if (!Window.Loaded)
            {
                Engine.ErrorQuit("Trying to instantiate a behaviour before Running the engine is strictly prohibited");
            }
        }

        protected Settings GlobalSettings => Settings.GlobalSettings;

        protected abstract void Load();
        protected abstract void Update();

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
    }
}
