using System;
using System.Collections.Generic;
using System.IO;

namespace UnicornHack.Utils
{
    public class CSScriptLoader<T>
        where T : ILoadable
    {
        private bool _allLoaded;

        public CSScriptLoader(string relativePath)
        {
            BasePath = Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        public string BasePath { get; }
        private Dictionary<string, T> NameLookup { get; } = new Dictionary<string, T>(StringComparer.Ordinal);

        public IEnumerable<T> GetAll()
        {
            if (!_allLoaded)
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
                _allLoaded = true;
            }

            return NameLookup.Values;
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

        private T Load(string path)
        {
            var variant = CSScriptDeserializer.LoadFile<T>(path);
            variant.OnLoad();
            NameLookup[variant.Name] = variant;
            return variant;
        }
    }
}