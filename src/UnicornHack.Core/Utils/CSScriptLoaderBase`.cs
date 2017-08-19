using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnicornHack.Utils
{
    public abstract class CSScriptLoaderBase<T> : CSScriptLoaderBase where T : ILoadable
    {
        protected Dictionary<string, T> NameLookup { get; } = new Dictionary<string, T>(StringComparer.Ordinal);
        protected string BasePath { get; }
        public string RelativePath { get; }
        public Type DataType { get; }
        protected string FilePattern { get; set; } = "*" + ScriptExtension;

        private readonly object _lockRoot = new object();

        // Loader shouldn't be declared on dataType to allow it to be loaded lazily
        protected CSScriptLoaderBase(string relativePath, Type dataType)
        {
            BasePath = Path.Combine(AppContext.BaseDirectory, relativePath);
            RelativePath = relativePath;
            DataType = dataType;
        }

        public T Get(string name)
        {
            EnsureLoaded();

            if (NameLookup.TryGetValue(name, out var variant))
            {
                return variant;
            }

            throw new InvalidOperationException($"'{name}' not found");
        }

        public virtual IEnumerable<T> GetAll()
        {
            EnsureLoaded();
            return NameLookup.Values;
        }

        protected virtual void EnsureLoaded()
        {
            if (NameLookup.Count == 0)
            {
                lock (_lockRoot)
                {
                    if (NameLookup.Count != 0)
                    {
                        return;
                    }

                    if (LoadScripts && Directory.Exists(BasePath))
                    {
                        foreach (var file in Directory.EnumerateFiles(BasePath, FilePattern,
                            SearchOption.TopDirectoryOnly))
                        {
                            try
                            {
                                var instance = LoadFile<T>(file);
                                NameLookup[instance.Name] = instance;
                            }
                            catch (Exception e)
                            {
                                throw new InvalidOperationException(
                                    $"Error while loading {typeof(T).Name} '{Path.GetFileName(file)}'\r\n", e);
                            }
                        }
                    }
                    else
                    {
                        foreach (var field in DataType.GetFields(BindingFlags.Public | BindingFlags.Static)
                            .Where(f => typeof(T).GetTypeInfo().IsAssignableFrom(f.FieldType)))
                        {
                            var instance = (T)field.GetValue(null);
                            NameLookup[instance.Name] = instance;
                        }
                    }
                }
            }
        }
    }
}