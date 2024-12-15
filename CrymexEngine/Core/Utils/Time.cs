namespace CrymexEngine
{
    public static class Time
    {
        public static float GameTime
        {
            get
            { 
                return _gameTime; 
            }
        }
        public static float DeltaTime
        {
            get
            {
                return _deltaTime;
            }
        }

        public static string CurrentTimeString
        {
            get
            {
                return $"{DateTime.Now.Hour}h:{DateTime.Now.Minute}m:{DateTime.Now.Second}s:{DateTime.Now.Millisecond}ms";
            }
        }

        public static string CurrentDateTimeShortString
        {
            get
            {
                return $"{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year} {DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}";
            }
        }

        private static float _gameTime;
        private static float _deltaTime;

        public static void Set(float deltaTime)
        {
            _gameTime += deltaTime;
            _deltaTime = deltaTime;
        }
    }
}
