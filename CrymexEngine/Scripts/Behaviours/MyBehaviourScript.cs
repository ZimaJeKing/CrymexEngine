using CrymexEngine;
using CrymexEngine.UI;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripting
{
    public class MyBehaviourScript : Behaviour
    {
        Entity car;
        Entity warning;

        AudioMixer mixer = new AudioMixer(0.25f, 1);

        public override void Load()
        {
            //car = Entity.GetEntity("Car");
            //warning = Entity.GetEntity("Warning");

            car = new Entity(Assets.GetTexture("WindowIcon"), Vector2.Zero, new Vector2(200), null, "Car");
            car.AddComponent<ClickableComponent>();
            car.renderer.Depth = 9959f;

            warning = new Entity(Assets.GetTexture("Warning"), new Vector2(128, 0), new Vector2(128));
            warning.AddComponent<ClickableComponent>();
            warning.renderer.Depth = -101f;
            warning.Parent = car;
        }

        public override void Update()
        {
            //car.Rotation = Window.gameTime * 30;
        }
    }

    public class ClickableComponent : EntityComponent
    {
        public override void OnMouseEnter()
        {
            renderer.color = Color4.LightGray;
        }
        public override void OnMouseExit()
        {
            renderer.color = Color4.White;
        }

        public override void OnMouseDown(MouseButton mouseButton)
        {
            renderer.color = Color4.Gray;
            //Debug.Log("Click");
        }

        public override void OnMouseHold(MouseButton mouseButton, float time)
        {
            if (mouseButton == MouseButton.Right)
            {
                float val = (MathF.Sin(Time.GameTime * 3) + 1) * 0.5f;
                renderer.color = new Color4(val, 0, val, 1f);
            }
        }
        public override void OnMouseUp()
        {
            renderer.color = Color4.LightGray;
        }
    }
}