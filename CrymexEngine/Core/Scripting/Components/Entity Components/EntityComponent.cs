using OpenTK.Mathematics;
using OpenTK.Platform.Windows;

namespace CrymexEngine
{
    public class EntityComponent : Component
    {
        public Entity Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                _entity = value;
                _renderer = value.GetComponent<EntityRenderer>();
            }
        }

        public EntityRenderer? Renderer
        {
            get
            {
                return (EntityRenderer?)_renderer;
            }
        }

        private Entity _entity;
    }
}
