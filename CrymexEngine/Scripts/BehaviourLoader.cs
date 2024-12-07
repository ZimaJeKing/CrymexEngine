namespace CrymexEngine.Scripting
{
    public static class BehaviourLoader
    {
        public static void LoadBehaviours()
        {
            // Add Your Behaviour Scripts Here //
            //              |                  //
            //              |                  //
            //             \ /                 //
            //              ˇ                  //

            Add<MyBehaviourScript>();

            // - - - - - - - - - - - - - - - - //
        }

        public static void Add<T>() where T : Behaviour
        {
            Behaviour? behaviour = (Behaviour?)Activator.CreateInstance(typeof(T));
            if (behaviour == null) return;

            Scene.current.behaviours.Add(behaviour);
            behaviour.Load();
        }
    }
}
