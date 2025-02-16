using CrymexEngine.Scripting;
using CrymexEngine.Scripts;

namespace CrymexEngine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Engine.Initialize();

            ScriptLoader.Add<MyBehaviourScript>();

            Engine.Run();
        }
    }
}
