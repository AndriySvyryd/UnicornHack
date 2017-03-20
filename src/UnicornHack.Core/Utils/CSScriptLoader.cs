using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnicornHack.Utils
{
    public class CSScriptLoader<T>
        where T : ILoadable
    {
        private IReadOnlyList<T> _objects;
        private readonly Dictionary<string, T> _nameLookup = new Dictionary<string, T>(StringComparer.Ordinal);

        public CSScriptLoader(string relativePath)
        {
            BasePath = Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        public string BasePath { get; }

        public IReadOnlyList<T> GetAll()
        {
            if (_objects == null)
            {
                foreach (var file in
                    Directory.EnumerateFiles(BasePath, CSScriptDeserializer.FilePattern, SearchOption.AllDirectories))
                {
                    if (!_nameLookup.ContainsKey(
                        CSScriptDeserializer.GetNameFromFilename(Path.GetFileNameWithoutExtension(file))))
                    {
                        Load(file);
                    }
                }
                _objects = _nameLookup.Values.ToList();
            }

            return _objects;
        }

        public T Get(string name)
        {
            if (_nameLookup.TryGetValue(name, out T variant))
            {
                return variant;
            }

            var path = Path.Combine(BasePath, CSScriptDeserializer.GetFilename(name));
            return !File.Exists(path) ? default(T) : Load(path);
        }

        private T Load(string path)
        {
            var instance = CSScriptDeserializer.LoadFile<T>(path);
            instance.OnLoad();
            _nameLookup[instance.Name] = instance;
            return instance;
        }
    }
}