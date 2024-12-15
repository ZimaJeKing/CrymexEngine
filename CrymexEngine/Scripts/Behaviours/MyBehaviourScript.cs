﻿using CrymexEngine.UI;
using OpenTK.Mathematics;
using System.Text;

namespace CrymexEngine.Scripting
{
    public class MyBehaviourScript : ScriptableBehaviour
    {
        Entity car;
        Entity beer;
        UIElement circle;
        UIElement weed;

        AudioMixer mixer = new AudioMixer(0.25f, 1);

        public override void Load()
        {
            weed = new UIElement(Assets.GetTexture("Weed"), -Window.HalfSize + new Vector2(64), new Vector2(128), null, 0);
            weed.AddComponent<UIClickableComponent>();

            circle = new UIElement(Assets.GetTexture("Circle"), Vector2.Zero, new Vector2(32));
            circle.Renderer.color = Color4.Red;
            circle.Renderer.color.A = 0.25f;
            circle.Renderer.Depth = -1;
            circle.interactible = false;

            car = new Entity(Assets.GetTexture("Car"), Vector2.Zero, new Vector2(256), null, "Car");
            car.AddComponent<ClickableComponent>();
            car.Renderer.Depth = 5;

            beer = new Entity(Assets.GetTexture("Beer"), new Vector2(350, 0), new Vector2(300));
            beer.AddComponent<ClickableComponent>();
            beer.Renderer.Depth = 0;
        }

        public override void Update()
        {
            if (Input.KeyDown(Key.W)) Audio.Play(Assets.GetAudioClip("SprayPaint"), 0.5f, mixer);

            circle.Position = Input.MousePosition;
            //car.Rotation = Window.gameTime * 30;

            car.Position += Axis2D.Arrows.GetValue() * 100 * Time.DeltaTime;
            Camera.position = car.Position;
        }
    }

    public class ClickableComponent : EntityComponent
    {
        public override void OnMouseEnter()
        {
            Renderer.color = Color4.LightGray;
        }
        public override void OnMouseExit()
        {
            Renderer.color = Color4.White;
        }

        public override void OnMouseDown(MouseButton mouseButton)
        {
            Renderer.color = Color4.Gray;
            //Debug.Log("Click");
        }

        public override void OnMouseHold(MouseButton mouseButton, float time)
        {
            if (mouseButton == MouseButton.Right)
            {
                float val = (MathF.Sin(time * 3) + 1) * 0.5f;
                Renderer.color = new Color4(val, 0, val, 1f);
            }
        }
        public override void OnMouseUp()
        {
            Renderer.color = Color4.LightGray;
        }
    }

    public class UIClickableComponent : UIComponent
    {
        public override void OnMouseEnter()
        {
            Renderer.color = Color4.LightGray;
        }
        public override void OnMouseExit()
        {
            Renderer.color = Color4.White;
        }

        public override void OnMouseDown(MouseButton mouseButton)
        {
            Renderer.color = Color4.Gray;
        }

        public override void OnMouseHold(MouseButton mouseButton, float time)
        {
            if (mouseButton == MouseButton.Right)
            {
                float val = (MathF.Sin(time * 3) + 1) * 0.5f;
                Renderer.color = new Color4(1, 1, 1, val);
            }
        }
        public override void OnMouseUp()
        {
            Renderer.color = Color4.LightGray;
        }
    }
}