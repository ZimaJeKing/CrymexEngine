using OpenTK.Mathematics;
using OpenTK.Platform.Windows;

namespace CrymexEngine
{
    public class EntityComponent : Component
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

        public override void PreRender() { }

        protected override void Load() { }

        protected override void Update() { }
    }
}
