using System.Reflection;
using CSharpScriptSerialization;

namespace UnicornHack.Utils.DataLoading;

public static class CSScriptLoaderHelpers
{
    public static readonly string ClassExtension = ".cs";
    public static readonly string ScriptExtension = ".csx";
    public static readonly bool LoadScripts = false;

    public static IReadOnlyList<string> GlobalNamespaces = new[]
    {
        typeof(object).GetTypeInfo().Namespace!,
        typeof(List<>).GetTypeInfo().Namespace!,
        typeof(Debug).GetTypeInfo().Namespace!,
        typeof(NotNullAttribute).GetTypeInfo().Namespace!,
        typeof(Enumerable).GetTypeInfo().Namespace!,
        typeof(StringBuilder).GetTypeInfo().Namespace!,
        typeof(Creature).GetTypeInfo().Namespace!,
        typeof(Effect).GetTypeInfo().Namespace!,
        typeof(MapFragment).GetTypeInfo().Namespace!,
        typeof(Direction).GetTypeInfo().Namespace!,
        typeof(Dimensions).GetTypeInfo().Namespace!
    };

    public static T Load<T>(string script)
        => CSScriptSerializer.Deserialize<T>(script, Enumerable.Empty<Assembly>(), GlobalNamespaces);

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
