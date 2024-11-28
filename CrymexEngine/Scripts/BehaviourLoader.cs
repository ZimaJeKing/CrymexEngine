namespace CrymexEngine.Scripts
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
            Behaviour? b = (Behaviour?)Activator.CreateInstance(typeof(T));
            if (b == null) return;
            Scene.Current.behaviours.Add(b);
        }
    }
}
