namespace CrymexEngine
{
    public abstract class Behaviour
    {
        public bool enabled = true;

        protected abstract void Load();
        protected abstract void Update();

        public static void LoadBehaviour(Behaviour behaviour)
        {
            if (behaviour == null) return;

            behaviour.Load();
        }

        public static void UpdateBehaviour(Behaviour behaviour)
        {
            if (behaviour == null) return;

            behaviour.Update();
        }
    }
}
