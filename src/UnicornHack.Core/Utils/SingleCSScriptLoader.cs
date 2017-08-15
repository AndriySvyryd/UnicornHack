using System;
using System.IO;

namespace UnicornHack.Utils
{
    public class SingleCSScriptLoader<T> : CSScriptLoaderBase
    {
        private T _object;
        public string BasePath { get; }
        public string Name { get; }
        public Type DataType { get; }

        public SingleCSScriptLoader(string relativePath, string name, Type dataType)
        {
            BasePath = Path.Combine(AppContext.BaseDirectory, relativePath);
            Name = name;
            DataType = dataType;
        }

        public T Object
        {
            get
            {
                if (Equals(_object, default(T)))
                {
                    _object = LoadScripts
                        ? LoadFile<T>(Path.Combine(BasePath, GetScriptFilename(Name)))
                        : LoadField<T>(DataType, GenerateIdentifier(Name));
                }
                return _object;
            }
        }
    }
}