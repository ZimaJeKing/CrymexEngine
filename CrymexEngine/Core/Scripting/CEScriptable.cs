using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class CEScriptable
    {
        public bool enabled = true;

        public virtual void Load() { }
        public virtual void Update() { }
        public virtual void PreRender() { }
        public virtual void TickLoop() { }
    }
}
