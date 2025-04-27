namespace CrymexEngine.Data
{
    public class Directories
    {
        public static string AssetsPath => _assetsPath;
        public static string RuntimeAssetsPath => _runtimeAssetsPath;
        public static string LogFolderPath => _logFolderPath;
        public static string SaveFolderPath => _saveFolderPath;

        private static string _assetsPath;
        private static string _runtimeAssetsPath;
        private static string _logFolderPath;
        private static string _saveFolderPath;

        internal static void Init()
        {
            _assetsPath = Engine.MainDirPath + "Assets" + Engine.PSep;
            _runtimeAssetsPath = Engine.MainDirPath + "Precompiled" + Engine.PSep;
            _logFolderPath = Engine.MainDirPath + "Logs" + Engine.PSep;
            _saveFolderPath = Engine.MainDirPath + "Saved" + Engine.PSep;
        }
    }
}
