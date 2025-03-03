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
                rendererComponent = value.GetComponent<EntityRenderer>();
            }
        }

        public EntityRenderer? Renderer
        {
            get
            {
                return (EntityRenderer?)rendererComponent;
            }
        }

        private Entity _entity;
    }
}
