using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Examples
{
    public class MusicPlayerExample : ScriptableBehaviour
    {
        UIElement playButton;

        Entity progressBar;

        bool playing = true;

        AudioSource source;

        public override void Load()
        {
            AudioClip clip = Assets.GetAudioClip("YourAudioClip");

            source = Audio.Play(clip, 0.1f, true, null);

            playButton = new UIElement(Assets.GetTexture("Play"), new Vector2(0, -100), new Vector2(200));
            Button buttonComponent = playButton.AddComponent<Button>();
            buttonComponent.hoverColor = new Color4(0, 0, 0, 210);
            buttonComponent.pressedColor = new Color4(0, 0, 0, 180);
            buttonComponent.onClick = PlayButtonClick;

            progressBar = new Entity(Texture.White, new Vector2(0, 256), new Vector2(512, 100));
            progressBar.Renderer.color = Color4.Red;
        }

        public override void Update()
        {
            if (!playing) return;
            progressBar.Scale = new Vector2(source.Progress * 512, 50);
            progressBar.Position = new Vector2(source.Progress * 256 - 256, 250);
        }

        void PlayButtonClick(MouseButton button)
        {
            if (playing)
            {
                source.Pause();
            }
            else
            {
                source.Play();
            }
            playing = !playing;
        }
    }
}
