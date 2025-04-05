using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class MusicPlayerAppExample : ScriptableBehaviour
    {
        UIElement buttonElement;
        Entity progressBar;

        bool playing = false;

        InputField inputField;
        AudioSource source;

        Texture playingTexture;
        Texture stoppedTexture;

        // Made for a 512x512 window
        protected override void Load()
        {
            playingTexture = Assets.GetTexture("Play");
            stoppedTexture = Assets.GetTexture("Stop");

            // Input field
            UIElement inputFieldElement = new UIElement(Texture.White, new Vector2(0, 100), new Vector2(256, 32));
            inputField = inputFieldElement.AddComponent<InputField>();
            inputField.CharacterLimit = 20;
            inputField.PreviewText = "Audio clip...";
            inputField.DisplayText.FontSize = 20;
            inputField.onSubmit = PlayClip;

            buttonElement = new UIElement(stoppedTexture, new Vector2(0, -100), new Vector2(200));
            buttonElement.cursorAlphaTest = false;
            Button buttonComponent = buttonElement.AddComponent<Button>();
            buttonComponent.hoverColor = new Color4(0, 0, 0, 210);
            buttonComponent.pressedColor = new Color4(0, 0, 0, 180);
            buttonComponent.onClick = PlayButtonClick;

            progressBar = new Entity(Texture.White, new Vector2(0, 256), new Vector2(512, 50));
            progressBar.Renderer.color = Color4.Red;
        }

        protected override void Update()
        {
            if (!playing) return;

            // Update the progress bar
            progressBar.Transform.Scale = new Vector2(source.Progress * 512, 50);
            progressBar.Transform.Position = new Vector2(source.Progress * 256 - 256, 250);
        }

        void PlayButtonClick(MouseButton button)
        {
            if (source == null)
            {
                PlayClip(inputField.Value);
                return;
            }

            if (playing)
            {
                buttonElement.Renderer.texture = stoppedTexture;
                source.Pause();
            }
            else
            {
                buttonElement.Renderer.texture = playingTexture;
                source.Play();
            }
            playing = !playing;
        }

        void PlayClip(string clip)
        {
            AudioClip audioClip = Assets.GetAudioClip(clip);
            if (audioClip == null) return;

            source?.Delete();

            source = new AudioSource(audioClip, 0.2f, true, false);
            source.Play();

            playing = true;
            buttonElement.Renderer.texture = playingTexture;
            TextEditor.Deselect();
        }
    }
}
