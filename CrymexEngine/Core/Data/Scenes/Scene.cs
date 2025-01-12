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

        public readonly List<Entity> entityDeleteQueue = new();
        public readonly List<UIElement> uiElementDeleteQueue = new();
        public readonly List<TextObject> textObjectDeleteQueue = new();

        private static Scene _current = new Scene();
        private static Scene? _nextToLoad = null;

        private bool _shouldClear = false;

        public Scene() { }

        /// <summary>
        /// Clear the scene and deletes loaded gameObjects and textObjects. QUEUED FOR NEXT FRAME
        /// </summary>
        public void Clear()
        {
            _shouldClear = true;
        }

        /// <summary>
        /// Sets the scene as current. QUEUED FOR NEXT FRAME
        /// </summary>
        public static void Load(Scene scene)
        {
            _nextToLoad = scene;
        }

        public static void UpdateQueues()
        {
            // Object deletion queues
            if (_current.entityDeleteQueue.Count > 0 || _current.uiElementDeleteQueue.Count > 0 || _current.textObjectDeleteQueue.Count > 0)
            {
                foreach (Entity entity in _current.entityDeleteQueue) { _current.entities.Remove(entity); }
                foreach (UIElement element in _current.uiElementDeleteQueue) { _current.uiElements.Remove(element); }
                foreach (TextObject text in _current.textObjectDeleteQueue) { _current.textObjects.Remove(text); }
                _current.entityDeleteQueue.Clear();
                _current.uiElementDeleteQueue.Clear();
                _current.textObjectDeleteQueue.Clear();
            }

            // Clearing Queue
            if (_current._shouldClear || _nextToLoad != null) _current.SafeClear();

            // Loading Queue
            if (_nextToLoad != null) _current = _nextToLoad;
        }

        private void SafeClear()
        {
            // Clear entities
            foreach (Entity entity in entities)
            {
                entity.Delete();
            }
            entities.Clear();

            // Clear elements
            foreach (UIElement element in uiElements)
            {
                element.Delete();
            }
            uiElements.Clear();

            // Clear textObjects
            foreach (TextObject text in textObjects)
            {
                text.Delete();
            }
            textObjects.Clear();

            // Clear behaviours and colliders
            scriptableBehaviours.Clear();
            colliders.Clear();
        }
    }
}
