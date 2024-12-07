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

        private static float _gameTime;
        private static float _deltaTime;

        public static void Set(float deltaTime)
        {
            _gameTime += deltaTime;
            _deltaTime = deltaTime;
        }
    }
}
