using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrymexEngine
{
    public class Component : Behaviour
    {
        protected Component? _renderer;

        /// <summary>
        /// Happens once when the behaviour is clicked
        /// </summary>
        public virtual void OnMouseDown(MouseButton mouseButton) { }
        public virtual void OnMouseHold(MouseButton mouseButton, float time) { }
        public virtual void OnMouseUp() { }
        public virtual void OnMouseEnter() { }
        public virtual void OnMouseStay(float time) { }
        public virtual void OnMouseExit() { }

        /// <summary>
        /// Used for setting custom shader parameters. Happens before every render operation
        /// </summary>
        public virtual void PreRender() { }
    }
}
