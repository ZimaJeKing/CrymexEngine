using CrymexEngine.Scenes;
using CrymexEngine.Scripts;
using CrymexEngine.Scripts.Examples;

namespace CrymexEngine.Scripting
{
    public static class ScriptLoader
    {
        public static void LoadBehaviours()
        {
            if (Window.Loaded) return;

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

            Scene.Current.scriptableBehaviours.Add(behaviour);
            Behaviour.LoadBehaviour(behaviour);
        }
    }
}
