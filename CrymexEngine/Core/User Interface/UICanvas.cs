namespace CrymexEngine.UI
{
    public static class UICanvas
    {
        public static void Update()
        {
            if (!Input.MouseDown(Mouse.Right) && !Input.MouseDown(Mouse.Left)) return;

            foreach (UIElement element in Scene.Current.uiElements)
            {

            }
        }
    }
}
