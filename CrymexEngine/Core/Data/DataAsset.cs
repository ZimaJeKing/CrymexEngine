using System;

namespace CrymexEngine.Data
{
    public class DataAsset
    {
        public string name;
        public string path;

        public DataAsset(string path)
        {
            this.path = path;
            if (Path.IsPathFullyQualified(path)) name = Path.GetFileNameWithoutExtension(path);
            else name = path;
        }
    }
}
