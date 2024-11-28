using CrymexEngine;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts
{
    public class MyBehaviourScript : Behaviour
    {
        Entity icon;
        Entity logo;

        AudioMixer mixer = new AudioMixer(0.25f, 1);

        public override void Load()
        {
            icon = new Entity(Assets.GetTexture("WindowIcon"), Vector2.Zero, new Vector2(100), "Car");

            logo = new Entity(Assets.GetTexture("Logo"), new Vector2(0, 0), new Vector2(100), "Logo");
            //logo.Parent = icon;
        }

        public override void TickLoop()
        {
        }

        public override void Update()
        {
            icon.renderer.depth = MathF.Sin(Window.gameTime * 30);
            //icon.renderer.color = new Color4(1f, 0f, 0f, (Sin(Application.gameTime * 30) + 1) * 0.5f);
        }
    }
}