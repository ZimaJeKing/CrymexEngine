namespace CrymexEngine
{
    public interface IMouseClick
    {
        /// <summary>
        /// Happens once when the behaviour is clicked
        /// </summary>
        public void OnMouseDown(MouseButton mouseButton);
        public void OnMouseHold(MouseButton mouseButton, float time);
        public void OnMouseUp();
    }
}
