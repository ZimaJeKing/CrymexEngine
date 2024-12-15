using CrymexEngine.UI;

namespace CrymexEngine
{
    public class Scene
    {
        public static Scene current;

        public List<Entity> entities = new List<Entity>();
        public List<ScriptableBehaviour> scriptableBehaviours = new List<ScriptableBehaviour>();
        public List<Collider> colliders = new List<Collider>();
        public List<UIElement> uiElements = new List<UIElement>();

        public Scene() { }
        public Scene(List<Entity> entities, List<ScriptableBehaviour> behaviours, List<Collider> colliders)
        {
            this.entities = entities;
            this.scriptableBehaviours = behaviours;
            this.colliders = colliders;
        }

        public void Clear()
        {
            entities = new List<Entity>();
            scriptableBehaviours = new List<ScriptableBehaviour>();
            colliders = new List<Collider>();
            uiElements = new List<UIElement>();
        }
    }
}
