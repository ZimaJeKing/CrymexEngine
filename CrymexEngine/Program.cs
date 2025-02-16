using CrymexEngine.Scripting;
using CrymexEngine.Scripts;

namespace CrymexEngine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ScriptLoader.Add<MyBehaviourScript>();
            Engine.Initialize();


            Engine.Run();
        }
    }
}
