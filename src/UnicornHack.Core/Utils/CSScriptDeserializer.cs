using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using UnicornHack.Models.GameDefinitions.Effects;

namespace UnicornHack.Utils
{
    public class CSScriptDeserializer
    {
        public static readonly string Extension = ".csx";

        public static T Load<T>(string script)
            => CSharpScript.EvaluateAsync<T>(script,
                ScriptOptions.Default.WithReferences(
                    typeof(T).GetTypeInfo().Assembly,
                    typeof(IReadOnlyList<>).GetTypeInfo().Assembly
#if !NET46
                    , Assembly.Load(new AssemblyName("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"))
#endif
                    )
                    .AddReferences(typeof(CSScriptDeserializer).GetTypeInfo()
                        .Assembly.GetReferencedAssemblies()
                        .Select(Assembly.Load))
                    .AddImports(
                        typeof(T).GetTypeInfo().Namespace,
                        typeof(AbilityEffect).GetTypeInfo().Namespace,
                        typeof(List<>).GetTypeInfo().Namespace,
                        typeof(DateTime).GetTypeInfo().Namespace)).GetAwaiter().GetResult();

        public static T LoadFile<T>(string path)
            => Load<T>(File.ReadAllText(path));

        public static string GetFilename(string name) => name.Replace(' ', '_') + Extension;
        public static string GetNameFromFilename(string filename) => filename.Replace('_', ' ') + Extension;
    }
}