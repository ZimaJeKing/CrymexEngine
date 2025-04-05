namespace CrymexEngine
{
    public abstract class Component : Behaviour
    {
        protected Component? rendererComponent;

        /// <summary>
        /// Used for setting custom shader parameters. Happens before every render operation
        /// </summary>
        public abstract void PreRender();

        protected override void OnQuit() { }
    }
}
