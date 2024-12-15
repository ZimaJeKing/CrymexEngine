namespace CrymexEngine.Scripting
{
    public static class ScriptLoader
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

        public static void Add<T>() where T : ScriptableBehaviour
        {
            ScriptableBehaviour? behaviour = (ScriptableBehaviour?)Activator.CreateInstance(typeof(T));
            if (behaviour == null) return;

            Scene.current.scriptableBehaviours.Add(behaviour);
            behaviour.Load();
        }
    }
}
