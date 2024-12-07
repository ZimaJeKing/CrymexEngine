using System;

namespace CrymexEngine.Data
{
    public class DataAsset
    {
        public string name;
        public int id;
        public string path;

        public DataAsset(string path)
        {
            this.path = path;
            name = Path.GetFileNameWithoutExtension(path);
        }
    }
}
