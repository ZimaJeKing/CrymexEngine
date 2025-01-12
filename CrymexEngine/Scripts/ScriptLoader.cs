using CrymexEngine.Scenes;
using CrymexEngine.Scripts;
using CrymexEngine.Examples;

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

            Add<MusicPlayerExample>();

            // - - - - - - - - - - - - - - - - //
        }

        public static void Add<T>() where T : ScriptableBehaviour
        {
            ScriptableBehaviour? behaviour = (ScriptableBehaviour?)Activator.CreateInstance(typeof(T));
            if (behaviour == null) return;

            Scene.Current.scriptableBehaviours.Add(behaviour);
            Behaviour.LoadBehaviour(behaviour);
        }
    }
}
