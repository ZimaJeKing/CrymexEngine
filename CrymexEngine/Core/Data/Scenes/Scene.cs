using CrymexEngine.UI;

namespace CrymexEngine.Scenes
{
    public class Scene
    {
        public static Scene current;

        public List<Entity> entities = new();
        public List<ScriptableBehaviour> scriptableBehaviours = new();
        public List<Collider> colliders = new();
        public List<UIElement> uiElements = new();
        public List<TextObject> textObjects = new();

        public List<Entity> entityDeleteQueue = new();
        public List<UIElement> uiElementDeleteQueue = new();
        public List<TextObject> textObjectDeleteQueue = new();

        public Scene() { }

        public void Clear()
        {
            UpdateDeleteQueue();

            entities = new List<Entity>();
            scriptableBehaviours = new List<ScriptableBehaviour>();
            colliders = new List<Collider>();
            uiElements = new List<UIElement>();
        }

        public void UpdateDeleteQueue()
        {
            foreach (Entity entity in entityDeleteQueue) { entities.Remove(entity); }
            foreach (UIElement element in uiElementDeleteQueue) { uiElements.Remove(element); }
            foreach (TextObject text in textObjectDeleteQueue) { textObjects.Remove(text); }
        }
    }
}
