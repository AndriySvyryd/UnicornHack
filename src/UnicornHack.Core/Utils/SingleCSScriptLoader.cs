using System;

namespace UnicornHack.Utils
{
    public class SingleCSScriptLoader<T> : CSScriptLoaderBase<T> where T : ILoadable, new()
    {
        private readonly string _name;

        public SingleCSScriptLoader(string relativePath, Type dataType) : base(relativePath, dataType)
        {
            _name = new T().Name;
            FilePattern = GetScriptFilename(_name) + ScriptExtension;
        }

        public T Object => Get(_name);
    }
}