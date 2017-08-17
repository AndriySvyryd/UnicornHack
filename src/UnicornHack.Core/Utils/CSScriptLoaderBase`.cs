using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace UnicornHack.Utils
{
    public abstract class CSScriptLoaderBase<T> : CSScriptLoaderBase
        where T : ILoadable
    {
        protected Dictionary<string, T> NameLookup { get; } = new Dictionary<string, T>(StringComparer.Ordinal);
        public string BasePath { get; }
        public Type DataType { get; }
        private readonly object _lockRoot = new object();

        protected CSScriptLoaderBase(string relativePath, Type dataType = null)
        {
            BasePath = Path.Combine(AppContext.BaseDirectory, relativePath);
            DataType = dataType;
        }

        public T Get(string name)
        {
            if (NameLookup.Count == 0)
            {
                LoadAll();
            }

            if (NameLookup.TryGetValue(name, out var variant))
            {
                return variant;
            }

            throw new InvalidOperationException($"'{name}' not found");
        }

        protected void LoadAll()
        {
            lock (_lockRoot)
            {
                if (NameLookup.Count != 0)
                {
                    return;
                }

                if (LoadScripts || DataType == null)
                {
                    foreach (var file in
                        Directory.EnumerateFiles(BasePath, FilePattern, SearchOption.AllDirectories))
                    {
                        try
                        {
                            var instance = LoadFile<T>(file);
                            instance.OnLoad();
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
                    foreach (var field in DataType.GetRuntimeFields())
                    {
                        var instance = (T)field.GetValue(null);
                        instance.OnLoad();
                        NameLookup[instance.Name] = instance;
                    }
                }
            }
        }
    }
}