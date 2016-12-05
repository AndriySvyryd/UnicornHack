using System.IO;
using System.Linq;
using System.Reflection;
using CSharpScriptSerialization;
using UnicornHack.Effects;

namespace UnicornHack.Utils
{
    public class CSScriptDeserializer
    {
        public static readonly string Extension = ".csx";

        public static T Load<T>(string script)
            => CSScriptSerializer.Deserialize<T>(
                script,
                Enumerable.Empty<Assembly>(),
                new[]
                {
                    typeof(Ability).GetTypeInfo().Namespace,
                    typeof(Effect).GetTypeInfo().Namespace
                });

        public static T LoadFile<T>(string path)
            => Load<T>(File.ReadAllText(path));

        public static string GetFilename(string name) => name.Replace(' ', '_') + Extension;
        public static string GetNameFromFilename(string filename) => filename.Replace('_', ' ') + Extension;
    }
}