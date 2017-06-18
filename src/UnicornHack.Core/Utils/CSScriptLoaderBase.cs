using System;
using System.Collections.Generic;
using System.IO;

namespace UnicornHack.Utils
{
    public abstract class CSScriptLoaderBase<T>
        where T : ILoadable
    {
        protected Dictionary<string, T> NameLookup { get; } = new Dictionary<string, T>(StringComparer.Ordinal);
        public string BasePath { get; }

        protected CSScriptLoaderBase(string relativePath)
        {
            BasePath = Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        public T Get(string name)
        {
            if (NameLookup.TryGetValue(name, out T variant))
            {
                return variant;
            }

            var path = Path.Combine(BasePath, CSScriptDeserializer.GetFilename(name));
            return !File.Exists(path) ? default(T) : Load(path);
        }

        protected void LoadAll()
        {
            foreach (var file in
                Directory.EnumerateFiles(BasePath, CSScriptDeserializer.FilePattern, SearchOption.AllDirectories))
            {
                if (!NameLookup.ContainsKey(
                    CSScriptDeserializer.GetNameFromFilename(Path.GetFileNameWithoutExtension(file))))
                {
                    Load(file);
                }
            }
        }

        protected T Load(string path)
        {
            var instance = CSScriptDeserializer.LoadFile<T>(path);
            instance.OnLoad();
            NameLookup[instance.Name] = instance;
            return instance;
        }
    }
}