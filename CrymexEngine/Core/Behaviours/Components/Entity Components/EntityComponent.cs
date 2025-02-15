namespace CrymexEngine
{
    public abstract class EntityComponent : Component
    {
        public Entity entity
        {
            get
            {
                return _entity;
            }
            set
            {
                _entity = value;
                _rendererComponent = value.GetComponent<EntityRenderer>();
            }
        }

        public EntityRenderer? Renderer
        {
            get
            {
                return (EntityRenderer?)_rendererComponent;
            }
        }

        private Entity _entity;
    }
}
