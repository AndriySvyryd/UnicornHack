using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpScriptSerialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using UnicornHack.Data.Branches;
using UnicornHack.Data.Items;
using UnicornHack.Definitions;
using UnicornHack.Generation;
using UnicornHack.Generation.Map;
using UnicornHack.Utils;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnicornHack.Editor
{
    public class Program
    {
        private static readonly bool SerializeToScript = false;

        public static void Main(string[] args)
        {
            //SerializeCreatures();
            //SerializePlayers();
            //SerializeItems();
            //SerializeItemGroups();
            SerializeBranches();
            //SerializeNormalFragments();
            //SerializeConnectingFragments();
            //SerializeDefiningFragments();
        }

        private static void SerializePlayers()
        {
            Console.WriteLine("Serializing players...");

            Directory.CreateDirectory(PlayerDirectory);

            foreach (var playerVariant in PlayerRace.Loader.GetAll())
            {
                var script = CSScriptSerializer.Serialize(playerVariant);

                File.WriteAllText(
                    Path.Combine(PlayerDirectory, CSScriptLoaderBase.GetScriptFilename(playerVariant.Name)),
                    script);

                if (SerializeToScript)
                {
                    Verify(script, playerVariant);
                }
            }
        }

        private static void SerializeCreatures()
        {
            Console.WriteLine("Serializing creatures...");

            Directory.CreateDirectory(CreatureDirectory);

            foreach (var creatureVariant in Creature.Loader.GetAll())
            {
                var script = CSScriptSerializer.Serialize(creatureVariant);

                File.WriteAllText(
                    Path.Combine(CreatureDirectory, CSScriptLoaderBase.GetScriptFilename(creatureVariant.Name)),
                    script);

                if (SerializeToScript)
                {
                    Verify(script, creatureVariant);
                }
            }
        }

        private static void SerializeItems()
        {
            Console.WriteLine("Serializing items...");

            Directory.CreateDirectory(ItemDirectory);

            foreach (var item in ItemVariant.Loader.GetAll())
            {
                var script = Serialize(item, ItemDirectory, typeof(ItemVariantData));

                if (SerializeToScript)
                {
                    Verify(script, item);
                }
            }
        }

        private static void SerializeItemGroups()
        {
            Directory.CreateDirectory(ItemGroupsDirectory);

            Serialize(ItemGroup.Loader.Object, ItemGroupsDirectory, ItemGroup.Loader.Name, ItemGroup.Loader.DataType);
        }

        private static void SerializeBranches()
        {
            Console.WriteLine("Serializing branches...");

            Directory.CreateDirectory(BranchDirectory);

            foreach (var branch in BranchDefinition.Loader.GetAll())
            {
                var script = Serialize(branch, BranchDirectory, typeof(BranchDefinitionData));

                if (SerializeToScript)
                {
                    Verify(script, branch);
                }
            }
        }

        private static void SerializeNormalFragments()
        {
            Console.WriteLine("Serializing normal fragments...");

            Directory.CreateDirectory(MapFragmentDirectory);

            foreach (var fragment in MapFragment.GetAllNormalMapFragments())
            {
                var script = CSScriptSerializer.Serialize(fragment);

                File.WriteAllText(
                    Path.Combine(MapFragmentDirectory, CSScriptLoaderBase.GetScriptFilename(fragment.Name)),
                    script);

                if (SerializeToScript)
                {
                    Verify(script, fragment);
                }
            }
        }

        private static void SerializeConnectingFragments()
        {
            Console.WriteLine("Serializing connecting fragments...");

            Directory.CreateDirectory(ConnectingMapFragmentDirectory);

            foreach (var fragment in ConnectingMapFragment.GetAllConnectingMapFragments())
            {
                var script = CSScriptSerializer.Serialize(fragment);

                File.WriteAllText(
                    Path.Combine(ConnectingMapFragmentDirectory, CSScriptLoaderBase.GetScriptFilename(fragment.Name)),
                    script);

                if (SerializeToScript)
                {
                    Verify(script, fragment);
                }
            }
        }

        private static void SerializeDefiningFragments()
        {
            Console.WriteLine("Serializing defining fragments...");

            Directory.CreateDirectory(DefiningMapFragmentDirectory);

            foreach (var fragment in DefiningMapFragment.GetAllDefiningMapFragments())
            {
                var script = CSScriptSerializer.Serialize(fragment);

                File.WriteAllText(
                    Path.Combine(DefiningMapFragmentDirectory, CSScriptLoaderBase.GetScriptFilename(fragment.Name)),
                    script);

                if (SerializeToScript)
                {
                    Verify(script, fragment);
                }
            }
        }

        private static string Serialize(ILoadable obj, string directory, Type dataType)
            => Serialize(obj, directory, obj.Name, dataType);

        private static string Serialize(object obj, string directory, string name, Type dataType)
        {
            if (CSScriptLoaderBase.LoadScripts)
            {
                var script = CSScriptSerializer.Serialize(obj);
                File.WriteAllText(Path.Combine(directory, CSScriptLoaderBase.GetScriptFilename(name)), script);
                return script;
            }
            else
            {
                var code = SerializeToCode(obj, name, dataType);
                File.WriteAllText(Path.Combine(directory, CSScriptLoaderBase.GetClassFilename(name)), code);
                return code;
            }
        }

        private static string SerializeToCode(object obj, string name, Type dataType)
        {
            var expression = CompilationUnit()
                .WithUsings(List(CSScriptLoaderBase.Namespaces.Select(n => UsingDirective(ParseName(n)))))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(ParseName(dataType.Namespace))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(ClassDeclaration(dataType.Name)
                                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                                        Token(SyntaxKind.StaticKeyword),
                                        Token(SyntaxKind.PartialKeyword)))
                                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                                        FieldDeclaration(
                                                VariableDeclaration(IdentifierName(obj.GetType().Name))
                                                    .WithVariables(SingletonSeparatedList(
                                                        VariableDeclarator(
                                                                Identifier(CSScriptLoaderBase.GenerateIdentifier(name)))
                                                            .WithInitializer(
                                                                EqualsValueClause(
                                                                    CSScriptSerializer.GetCreationExpression(obj))))))
                                            .WithModifiers(
                                                TokenList(Token(SyntaxKind.PublicKeyword),
                                                    Token(SyntaxKind.StaticKeyword),
                                                    Token(SyntaxKind.ReadOnlyKeyword)))))))));

            using (var workspace = new AdhocWorkspace())
            {
                return Formatter.Format(
                        expression,
                        workspace,
                        workspace.Options)
                    .ToFullString();
            }
        }

        private static void Verify(string script, Creature creature)
            => Verify<Creature>(script, c => c.Name == creature.Name,
                c => c.SimpleProperties, c => c.ValuedProperties);

        private static void Verify(string script, PlayerRace player)
            => Verify<PlayerRace>(script, c => c.Name == player.Name, null, null);

        private static void Verify(string script, ItemVariant item)
            => Verify<Item>(script, c => c.Name == item.Name,
                c => c.SimpleProperties, c => c.ValuedProperties);

        private static void Verify(string script, BranchDefinition branch)
            => Verify<Branch>(script, f => f.Name == branch.Name, null, null);

        private static void Verify(string script, MapFragment fragment)
            => Verify<MapFragment>(script, f => f.Name == fragment.Name && VerifyNoUnicode(fragment), null, null);

        private static void Verify(string script, ConnectingMapFragment fragment)
            => Verify<ConnectingMapFragment>(script, f => f.Name == fragment.Name && VerifyNoUnicode(fragment), null,
                null);

        private static void Verify(string script, DefiningMapFragment fragment)
            => Verify<DefiningMapFragment>(script, f => f.Name == fragment.Name && VerifyNoUnicode(fragment), null,
                null);

        private static bool VerifyNoUnicode(MapFragment fragment)
        {
            int x = 0, y = 0;
            foreach (var character in fragment.Map)
            {
                switch (character)
                {
                    case '\r':
                        continue;
                    case '\n':
                        x = 0;
                        y++;
                        continue;
                }

                if (character != (byte)character)
                {
                    throw new InvalidOperationException($"Invalid character '{character}' at {x},{y}");
                }
                x++;
            }

            return true;
        }

        private static void Verify<T>(string script, Func<T, bool> isValid, Func<T, ISet<string>> getSimpleProperties,
            Func<T, IDictionary<string, object>> getValuedProperties)
        {
            try
            {
                var serializedVariant = CSScriptLoaderBase.Load<T>(script);
                if (!isValid(serializedVariant))
                {
                    Console.WriteLine(script);
                }

                var simpleProperties = getSimpleProperties?.Invoke(serializedVariant);
                if (simpleProperties != null)
                {
                    foreach (var simpleProperty in simpleProperties)
                    {
                        if (!CustomProperties.TryGetValue(simpleProperty, out PropertyDescription description))
                        {
                            throw new InvalidOperationException(
                                "Invalid simple property: " + simpleProperty);
                        }
                        if (description.PropertyType != typeof(bool))
                        {
                            throw new InvalidOperationException(
                                $"Simple property {simpleProperty} should be of type {description.PropertyType}");
                        }
                    }
                }

                var valuedProperties = getValuedProperties?.Invoke(serializedVariant);
                if (valuedProperties != null)
                {
                    foreach (var valuedProperty in valuedProperties)
                    {
                        if (!CustomProperties.TryGetValue(valuedProperty.Key,
                            out PropertyDescription description))
                        {
                            throw new InvalidOperationException("Invalid valued property: " + valuedProperty);
                        }
                        if (description.PropertyType != valuedProperty.Value.GetType())
                        {
                            throw new InvalidOperationException(
                                $"Valued property {valuedProperty} should be of type {description.PropertyType}");
                        }

                        if (((IComparable)description.MinValue)?.CompareTo(valuedProperty.Value) > 0)
                        {
                            throw new InvalidOperationException(
                                $"Valued property {valuedProperty} should be lesser or equal to " +
                                description.MinValue);
                        }
                        if (((IComparable)description.MaxValue)?.CompareTo(valuedProperty.Value) < 0)
                        {
                            throw new InvalidOperationException(
                                $"Valued property {valuedProperty} should be greater or equal to " +
                                description.MaxValue);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine(script);
                throw;
            }
        }

        private static readonly Dictionary<string, PropertyDescription> CustomProperties = GetCustomProperties();

        private static Dictionary<string, PropertyDescription> GetCustomProperties()
            => typeof(PropertyDescription).GetProperties(
                    BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static)
                .Where(p => !p.CanWrite)
                .ToDictionary(
                    p => p.Name,
                    p => (PropertyDescription)p.GetGetMethod().Invoke(null, null));

        public static readonly string BaseDirectory =
            GetCommonPrefix(new[]
            {
                ItemGroup.Loader.BasePath,
                MapFragment.NormalLoader.BasePath
            });

        public static readonly string PlayerDirectory =
            Path.Combine(BaseDirectory, "new",
                PlayerRace.Loader.BasePath.Substring(BaseDirectory.Length,
                    PlayerRace.Loader.BasePath.Length - BaseDirectory.Length));

        public static readonly string CreatureDirectory =
            Path.Combine(BaseDirectory, "new",
                Creature.Loader.BasePath.Substring(BaseDirectory.Length,
                    Creature.Loader.BasePath.Length - BaseDirectory.Length));

        public static readonly string ItemDirectory =
            Path.Combine(BaseDirectory, "new",
                ItemVariant.Loader.BasePath.Substring(BaseDirectory.Length,
                    ItemVariant.Loader.BasePath.Length - BaseDirectory.Length));

        public static readonly string ItemGroupsDirectory =
            Path.Combine(BaseDirectory, "new",
                ItemGroup.Loader.BasePath.Substring(BaseDirectory.Length,
                    ItemGroup.Loader.BasePath.Length - BaseDirectory.Length));

        public static readonly string BranchDirectory =
            Path.Combine(BaseDirectory, "new",
                BranchDefinition.Loader.BasePath.Substring(BaseDirectory.Length,
                    BranchDefinition.Loader.BasePath.Length - BaseDirectory.Length));

        public static readonly string MapFragmentDirectory =
            Path.Combine(BaseDirectory, "new",
                MapFragment.NormalLoader.BasePath.Substring(BaseDirectory.Length,
                    MapFragment.NormalLoader.BasePath.Length - BaseDirectory.Length));

        public static readonly string ConnectingMapFragmentDirectory =
            Path.Combine(BaseDirectory, "new",
                ConnectingMapFragment.ConnectingLoader.BasePath.Substring(BaseDirectory.Length,
                    ConnectingMapFragment.ConnectingLoader.BasePath.Length - BaseDirectory.Length));

        public static readonly string DefiningMapFragmentDirectory =
            Path.Combine(BaseDirectory, "new",
                DefiningMapFragment.DefiningLoader.BasePath.Substring(BaseDirectory.Length,
                    DefiningMapFragment.DefiningLoader.BasePath.Length - BaseDirectory.Length));

        private static string GetCommonPrefix(IReadOnlyList<string> strings)
        {
            if (strings.Count == 0)
            {
                return null;
            }

            var firstString = strings[0];
            var prefixLength = firstString.Length;

            for (var y = 1; y < strings.Count; y++)
            {
                var s = strings[y];
                for (var i = 0; i < firstString.Length && i < prefixLength; i++)
                {
                    var c = firstString[i];
                    if (i == s.Length
                        || s[i] != c)
                    {
                        prefixLength = i;
                    }
                }
            }

            return firstString.Substring(0, prefixLength);
        }
    }
}