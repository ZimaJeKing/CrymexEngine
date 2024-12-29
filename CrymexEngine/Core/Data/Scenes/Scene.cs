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

        public Scene() { }

        public void Clear()
        {
            entities = new List<Entity>();
            scriptableBehaviours = new List<ScriptableBehaviour>();
            colliders = new List<Collider>();
            uiElements = new List<UIElement>();
        }
    }
}
