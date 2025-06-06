﻿using CrymexEngine.Scripting;
using CrymexEngine.Scripts;
using CrymexEngine.Scripts.Examples;

namespace CrymexEngine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Engine.Initialize();

            ScriptLoader.Add<MusicPlayerAppExample>();

            Engine.Run();
        }
    }
}
