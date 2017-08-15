using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpScriptSerialization;
using UnicornHack.Definitions;
using UnicornHack.Effects;
using UnicornHack.Generation;
using UnicornHack.Generation.Map;

namespace UnicornHack.Utils
{
    public abstract class CSScriptLoaderBase
    {
        public static readonly string ClassExtension = ".cs";
        public static readonly string ScriptExtension = ".csx";
        public static readonly string FilePattern = "*" + ScriptExtension;
        public static readonly bool LoadScripts = false;

        public static IReadOnlyList<string> Namespaces = new[]
        {
            typeof(PlayerRace).GetTypeInfo().Namespace,
            typeof(Ability).GetTypeInfo().Namespace,
            typeof(Effect).GetTypeInfo().Namespace,
            typeof(Weight).GetTypeInfo().Namespace,
            typeof(MapFragment).GetTypeInfo().Namespace,
            typeof(Dimensions).GetTypeInfo().Namespace
        };

        public static T Load<T>(string script)
            => CSScriptSerializer.Deserialize<T>(
                script,
                Enumerable.Empty<Assembly>(),
                Namespaces);

        public static T LoadFile<T>(string path)
            => Load<T>(File.ReadAllText(path));

        public static string GetScriptFilename(string name) => name.Replace(' ', '_') + ScriptExtension;
        public static string GetNameFromFilename(string filename) => filename.Replace('_', ' ') + ScriptExtension;
        public static string GetClassFilename(string name) => GenerateIdentifier(name) + ClassExtension;

        public static string GenerateIdentifier(string name)
        {
            var candidateStringBuilder = new StringBuilder();
            var previousLetterCharInWordIsLowerCase = false;
            var isFirstCharacterInWord = true;
            foreach (var c in name)
            {
                var isNotLetterOrDigit = !char.IsLetterOrDigit(c);
                if (isNotLetterOrDigit
                    || (previousLetterCharInWordIsLowerCase && char.IsUpper(c)))
                {
                    isFirstCharacterInWord = true;
                    previousLetterCharInWordIsLowerCase = false;
                    if (isNotLetterOrDigit)
                    {
                        continue;
                    }
                }

                candidateStringBuilder.Append(isFirstCharacterInWord
                    ? char.ToUpperInvariant(c)
                    : char.ToLowerInvariant(c));
                isFirstCharacterInWord = false;
                if (char.IsLower(c))
                {
                    previousLetterCharInWordIsLowerCase = true;
                }
            }

            return candidateStringBuilder.ToString();
        }

        protected T LoadField<T>(Type type, string fieldName)
            => (T)type.GetRuntimeField(fieldName).GetValue(null);
    }
}