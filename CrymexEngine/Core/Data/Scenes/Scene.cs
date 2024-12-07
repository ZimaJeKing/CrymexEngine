using CrymexEngine.UI;

namespace CrymexEngine
{
    public class Scene
    {
        public static Scene current;

        public List<Entity> entities = new List<Entity>();
        public List<Behaviour> behaviours = new List<Behaviour>();
        public List<Collider> colliders = new List<Collider>();
        public List<UIElement> uiElements = new List<UIElement>();

        public Scene() { }
        public Scene(List<Entity> entities, List<Behaviour> behaviours, List<Collider> colliders)
        {
            this.entities = entities;
            this.behaviours = behaviours;
            this.colliders = colliders;
        }

        public void Clear()
        {
            entities = new List<Entity>();
            behaviours = new List<Behaviour>();
            colliders = new List<Collider>();
            uiElements = new List<UIElement>();
        }
    }
}
