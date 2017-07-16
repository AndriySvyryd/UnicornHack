using System;
using System.IO;

namespace UnicornHack.Utils
{
    public class SingleCSScriptLoader<T>
    {
        private T _object;
        public string ScriptPath { get; }

        public SingleCSScriptLoader(string relativePath)
        {
            ScriptPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        public T Object
        {
            get
            {
                if (Equals(_object, default(T)))
                {
                    _object = CSScriptDeserializer.LoadFile<T>(ScriptPath);
                }
                return _object;
            }
        }
    }
}