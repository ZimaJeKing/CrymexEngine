using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class EntityComponent : CEScriptable
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
                renderer = Entity.GetComponent<Renderer>();
            }
        }

        public Renderer? renderer { get; private set; }

        private Entity _entity;

        public void Init(Entity entity)
        {
            _entity = entity;
            renderer = renderer;
        }
    }
}
