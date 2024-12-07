using OpenTK.Mathematics;
using OpenTK.Platform.Windows;

namespace CrymexEngine
{
    public class EntityComponent : Behaviour
    {
        public Entity Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                Bind(value);
            }
        }

        protected Renderer? renderer;

        private Entity _entity;

        public void Bind(Entity entity)
        {
            _entity = entity;
            renderer = entity.GetComponent<Renderer>();
        }

        /// <summary>
        /// Happens once when the behaviour is clicked
        /// </summary>
        public virtual void OnMouseDown(MouseButton mouseButton) { }
        public virtual void OnMouseHold(MouseButton mouseButton, float time) { }
        public virtual void OnMouseUp() { }
        public virtual void OnMouseEnter() { }
        public virtual void OnMouseStay(float time) { }
        public virtual void OnMouseExit() { }
    }
}
