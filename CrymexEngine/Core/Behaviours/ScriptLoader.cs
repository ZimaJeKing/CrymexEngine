using CrymexEngine.Scenes;
using CrymexEngine.Scripts;
using CrymexEngine.Scripts.Examples;

namespace CrymexEngine.Scripting
{
    public static class ScriptLoader
    {
        private static List<Type> _startingBehaviours = new();

        internal static void LoadBehaviours()
        {
            foreach (Type type in _startingBehaviours)
            {
                RuntimeAdd(type);
            }
        }

        public static void Add<T>() where T : ScriptableBehaviour
        {
            if (!Window.Loaded)
            {
                if (Engine.Initialized)
                {
                    _startingBehaviours.Add(typeof(T));
                }
                else
                {
                    Debug.LogError("[Behaviour Loader] Adding behaviours before initializing the engine is strictly prohibited");
                }
            }
            else
            {
                Debug.LogWarning("[Behaviour Loader] For adding behaviours during runtime use the RuntimeAdd method");
                RuntimeAdd<T>();
            }
        }

        public static T? RuntimeAdd<T>() where T : ScriptableBehaviour
        {
            if (!Window.Loaded || !Engine.Initialized)
            {
                _startingBehaviours.Add(typeof(T));
            }

            T? behaviour = (T?)Activator.CreateInstance(typeof(T));
            if (behaviour == null) return null;

            Scene.Current.scriptableBehaviours.Add(behaviour);
            Behaviour.LoadBehaviour(behaviour);
            return behaviour;
        }

        public static ScriptableBehaviour RuntimeAdd(Type type)
        {
            if (!type.IsSubclassOf(typeof(ScriptableBehaviour)))
            {
                Debug.LogError($"[Behaviour Loader] Type '{type}' is not a subclass of ScriptableBehaviour");
                return null;
            }

            ScriptableBehaviour? behaviour = (ScriptableBehaviour?)Activator.CreateInstance(type);
            if (behaviour == null) return null;

            Scene.Current.scriptableBehaviours.Add(behaviour);
            Behaviour.LoadBehaviour(behaviour);
            return behaviour;
        }
    }
}
