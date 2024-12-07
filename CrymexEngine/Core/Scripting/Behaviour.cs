using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class Behaviour
    {
        public bool enabled = true;

        public virtual void Load() { }
        public virtual void Update() { }
        public virtual void PreRender() { }
    }
}
