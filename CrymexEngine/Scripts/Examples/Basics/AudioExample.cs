using CrymexEngine.Audio;
using CrymexEngine.UI;
using OpenTK.Mathematics;

namespace CrymexEngine.Scripts.Examples
{
    public class AudioExample : ScriptableBehaviour
    {
        float volume = 0.25f;
        AudioClip audioClip;

        protected override void Load()
        {
            string audioClipName = "SprayPaint";

            audioClip = Assets.GetAudioClip(audioClipName);
            if (audioClip == null)
            {
                Debug.Log("Audio clip not found");
            }

            UIElement buttonElement = new UIElement(Texture.White, Vector2.Zero, new Vector2(256, 64), null, "Button");
            Button button = buttonElement.AddComponent<Button>();
            button.onClick = ButtonClick;

            TextObject buttonText = new TextObject(Vector2.Zero, new Vector2i(256, 64), "Click to make sound", Assets.DefaultFontFamily, 0);
        }

        protected override void Update()
        {

        }

        private void ButtonClick(MouseButton mouseButton)
        {
            PlayAudio();
        }

        private void PlayAudio()
        {
            ALMgr.Play(audioClip, volume);
        }
    }
}
