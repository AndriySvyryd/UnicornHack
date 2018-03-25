using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UnicornHack.Utils.DataLoading
{
    public abstract class CSScriptLoaderBase<T> : Loader<T>
        where T : class, ILoadable
    {
        protected string BasePath { get; }
        public string RelativePath { get; }
        public Type DataType { get; }
        protected string FilePattern { get; set; } = "*" + CSScriptLoaderHelpers.ScriptExtension;

        private readonly object _lockRoot = new object();

        // Loader shouldn't be declared on dataType to allow it to be loaded lazily
        protected CSScriptLoaderBase(string relativePath, Type dataType)
        {
            BasePath = Path.Combine(AppContext.BaseDirectory, relativePath);
            RelativePath = relativePath;
            DataType = dataType;
        }

        protected override void EnsureLoaded()
        {
            if (NameLookup.Count != 0)
            {
                return;
            }

            lock (_lockRoot)
            {
                if (NameLookup.Count != 0)
                {
                    return;
                }

                if (CSScriptLoaderHelpers.LoadScripts && Directory.Exists(BasePath))
                {
                    foreach (var file in Directory.EnumerateFiles(
                        BasePath, FilePattern, SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            var instance = CSScriptLoaderHelpers.LoadFile<T>(file);
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
