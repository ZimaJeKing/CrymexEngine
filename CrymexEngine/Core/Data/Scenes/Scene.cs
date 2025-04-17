using CrymexEngine.AetherPhysics;
using CrymexEngine.UI;

namespace CrymexEngine.Scenes
{
    public class Scene
    {
        public static Scene Current => _current;

        public readonly List<Entity> entities = new();
        public readonly List<ScriptableBehaviour> scriptableBehaviours = new();
        public readonly List<Collider> colliders = new();
        public readonly List<UIElement> uiElements = new();
        public readonly List<TextObject> textObjects = new();
        public readonly List<LineGroup> lines = new();

        public readonly List<Entity> entityDeleteQueue = new();
        public readonly List<UIElement> uiElementDeleteQueue = new();
        public readonly List<LineGroup> lineDeleteQueue = new();
        public readonly List<TextObject> textObjectDeleteQueue = new();

        private static Scene _current = new Scene();
        private static Scene? _nextToLoad = null;

        public Scene() { }

        /// <summary>
        /// Clear the scene and deletes loaded gameObjects and textObjects. QUEUED FOR NEXT FRAME
        /// </summary>
        public void Clear()
        {
            // Clear entities
            entities.Clear();

            // Clear elements
            uiElements.Clear();

            // Clear textObjects
            textObjects.Clear();

            // Clear colliders
            colliders.Clear();

            // Clear lines
            lines.Clear();

            // Clear scriptable behaviours
            for (int i = 0; i < scriptableBehaviours.Count; i++)
            {
                ScriptableBehaviour b = scriptableBehaviours[i];
                if (!b.stayAlive) scriptableBehaviours.Remove(b);
            }
        }

        /// <summary>
        /// Sets the scene as current. QUEUED FOR NEXT FRAME
        /// </summary>
        public static void Load(Scene scene)
        {
            _nextToLoad = scene;
        }

        internal static void LoadImmediate(Scene scene)
        {
            _current = scene;
        }

        internal static void UpdateQueues()
        {
            // Object deletion queues
            if (_current.entityDeleteQueue.Count > 0 || _current.uiElementDeleteQueue.Count > 0 || _current.textObjectDeleteQueue.Count > 0 || _current.lineDeleteQueue.Count > 0)
            {
                foreach (Entity entity in _current.entityDeleteQueue) { _current.entities.Remove(entity); }
                foreach (UIElement element in _current.uiElementDeleteQueue) { _current.uiElements.Remove(element); }
                foreach (TextObject text in _current.textObjectDeleteQueue) { _current.textObjects.Remove(text); }
                foreach (LineGroup line in _current.lineDeleteQueue) { _current.lines.Remove(line); }
                _current.entityDeleteQueue.Clear();
                _current.uiElementDeleteQueue.Clear();
                _current.textObjectDeleteQueue.Clear();
            }

            // Loading Queue
            if (_nextToLoad != null) _current = _nextToLoad;
        }
    }
}
