using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpScriptSerialization;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;
using UnicornHack.Generation.Map;

namespace UnicornHack.Utils
{
    public abstract class CSScriptLoaderBase
    {
        public static readonly string ClassExtension = ".cs";
        public static readonly string ScriptExtension = ".csx";
        protected static readonly bool LoadScripts = false;

        public static IReadOnlyList<string> Namespaces = new[]
        {
            typeof(object).GetTypeInfo().Namespace,
            typeof(List<>).GetTypeInfo().Namespace,
            typeof(Game).GetTypeInfo().Namespace,
            typeof(Ability).GetTypeInfo().Namespace,
            typeof(Effect).GetTypeInfo().Namespace,
            typeof(Weight).GetTypeInfo().Namespace,
            typeof(MapFragment).GetTypeInfo().Namespace,
            typeof(Dimensions).GetTypeInfo().Namespace
        };

        public static T Load<T>(string script)
            => CSScriptSerializer.Deserialize<T>(script, Enumerable.Empty<Assembly>(), Namespaces);

        public static T LoadFile<T>(string path) => Load<T>(File.ReadAllText(path));

        public static string GetScriptFilename(string name) => name.Replace(' ', '_') + ScriptExtension;
        public static string GetNameFromFilename(string filename) => filename.Replace('_', ' ') + ScriptExtension;
        public static string GetClassFilename(string name) => GenerateIdentifier(name) + ClassExtension;

        public static string GenerateIdentifier(string name)
        {
            var candidateStringBuilder = new StringBuilder();
            var isFirstCharacterInWord = true;
            foreach (var c in name)
            {
                var isNotLetterOrDigit = !char.IsLetterOrDigit(c);
                if (isNotLetterOrDigit || char.IsUpper(c))
                {
                    isFirstCharacterInWord = true;
                    if (isNotLetterOrDigit)
                    {
                        continue;
                    }
                }

                candidateStringBuilder.Append(isFirstCharacterInWord
                    ? char.ToUpperInvariant(c)
                    : char.ToLowerInvariant(c));
                isFirstCharacterInWord = false;
            }

            return candidateStringBuilder.ToString();
        }

        public static string GetNameFromIdentifier(string identifier)
        {
            var candidateStringBuilder = new StringBuilder();
            var charsInPreviousWord = 0;
            foreach (var c in identifier)
            {
                if (char.IsUpper(c) && charsInPreviousWord > 1)
                {
                    candidateStringBuilder.Append(' ');
                    charsInPreviousWord = 0;
                }

                candidateStringBuilder.Append(char.ToLowerInvariant(c));
                charsInPreviousWord++;
            }

            return candidateStringBuilder.ToString();
        }
    }
}