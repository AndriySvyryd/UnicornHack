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
            SerializeCreatures();
            SerializePlayers();
            SerializeItems();
            SerializeItemGroups();
            SerializeBranches();
            SerializeNormalFragments();
            SerializeConnectingFragments();
            SerializeDefiningFragments();
        }

        private static void SerializePlayers()
        {
            Console.WriteLine("Serializing players...");

            Serialize(PlayerRace.Loader);
        }

        private static void SerializeCreatures()
        {
            Console.WriteLine("Serializing creatures...");

            Serialize(Creature.Loader);
        }

        private static void SerializeItems()
        {
            Console.WriteLine("Serializing items...");

            Serialize(ItemVariant.Loader);
        }

        private static void SerializeItemGroups()
        {
            Console.WriteLine("Serializing item groups...");

            Serialize(ItemGroup.Loader);
        }

        private static void SerializeBranches()
        {
            Console.WriteLine("Serializing branches...");

            Serialize(BranchDefinition.Loader);
        }

        private static void SerializeNormalFragments()
        {
            Console.WriteLine("Serializing normal fragments...");

            Serialize(MapFragment.Loader);
        }

        private static void SerializeConnectingFragments()
        {
            Console.WriteLine("Serializing connecting fragments...");

            Serialize(ConnectingMapFragment.Loader);
        }

        private static void SerializeDefiningFragments()
        {
            Console.WriteLine("Serializing defining fragments...");

            Serialize(DefiningMapFragment.Loader);
        }

        private static void Serialize<T>(CSScriptLoaderBase<T> loader)
            where T : ILoadable
        {
            var directory = Path.Combine(AppContext.BaseDirectory, "New", loader.RelativePath);
            Directory.CreateDirectory(directory);
            foreach (var item in loader.GetAll())
            {
                if (SerializeToScript)
                {
                    var script = CSScriptSerializer.Serialize(item);
                    File.WriteAllText(Path.Combine(loader.RelativePath, CSScriptLoaderBase.GetScriptFilename(item.Name)), script);
                    Verify(script, (dynamic)item);
                }
                else
                {
                    var code = SerializeToCode(item, item.Name, loader.DataType);
                    File.WriteAllText(Path.Combine(directory, CSScriptLoaderBase.GetClassFilename(item.Name)), code);
                }
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
    }
}