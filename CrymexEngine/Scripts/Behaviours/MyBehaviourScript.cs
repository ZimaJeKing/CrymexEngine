using CrymexEngine;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts
{
    public class MyBehaviourScript : Behaviour
    {
        Entity circle;
        Entity icon;
        Entity warning;

        public override void Load()
        {
            circle = new Entity(Assets.GetTexture("Circle"), Vector2.Zero, new Vector2(256));
            icon = new Entity(Assets.GetTexture("WindowIcon"), Vector2.Zero, new Vector2(256));
            warning = new Entity(Assets.GetTexture("Warning"), Vector2.Zero, new Vector2(100));

            warning.localPosition = new Vector2(0, 100);
            icon.Bind(warning);
        }

        public override void TickLoop()
        {
        }

        public override void Update()
        {
            icon.rotation = -Program.gameTime % 360;
        }
    }
}