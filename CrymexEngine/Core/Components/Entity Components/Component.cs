using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class Component : CEScriptable
    {
        public Entity entity;
        public Renderer renderer;
        public bool enabled = true;
    }
}
