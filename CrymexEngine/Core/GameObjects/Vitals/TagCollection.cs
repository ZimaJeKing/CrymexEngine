using CrymexEngine.Utils;

namespace CrymexEngine.GameObjects
{
    internal class TagCollection
    {
        private List<string> _tags = new();

        public TagCollection() 
        { 

        }

        public bool HasTag(string tag) => _tags.Contains(tag);

        public bool AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || string.IsNullOrWhiteSpace(tag) || DataUtil.ContainsSpecialCharacters(tag)) return false;

            _tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag) => _tags.Remove(tag);
    }
}
