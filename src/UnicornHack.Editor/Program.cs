using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpScriptSerialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
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
            Serialize(PropertyDescription.Loader);
            Serialize(CreatureVariant.Loader);
            Serialize(PlayerRaceDefinition.Loader);
            Serialize(ItemVariant.Loader);
            Serialize(ItemGroup.Loader);
            Serialize(BranchDefinition.Loader);
            Serialize(NormalMapFragment.Loader);
            Serialize(ConnectingMapFragment.Loader);
            Serialize(DefiningMapFragment.Loader);
        }

        private static void Serialize<T>(CSScriptLoaderBase<T> loader, Func<T, T> transform = null)
            where T : ILoadable
        {
            Console.WriteLine("Serializing " + typeof(T).Name + " instances...");

            var directory = Path.Combine(AppContext.BaseDirectory, "New", loader.RelativePath);
            Directory.CreateDirectory(directory);
            foreach (var item in loader.GetAll())
            {
                try
                {
                    var itemToSerialize = transform != null ? transform(item) : item;
                    string script = null;
                    if (SerializeToScript)
                    {
                        script = CSScriptSerializer.Serialize(itemToSerialize);
                        File.WriteAllText(
                            Path.Combine(loader.RelativePath,
                                CSScriptLoaderBase.GetScriptFilename(itemToSerialize.Name)), script);
                    }
                    else
                    {
                        var code = SerializeToCode(itemToSerialize, itemToSerialize.Name, loader.DataType);
                        File.WriteAllText(
                            Path.Combine(directory, CSScriptLoaderBase.GetClassFilename(itemToSerialize.Name)), code);
                    }
                    Verify(script, (dynamic)item);
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to serialize " + item.Name, e);
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

        private static void Verify(string script, PropertyDescription property)
            => Verify(script, property, i => i.Name == property.Name, null, null);

        private static void Verify(string script, CreatureVariant creature)
            => Verify(script, creature, c => c.Name == creature.Name, c => c.SimpleProperties, c => c.ValuedProperties);

        private static void Verify(string script, PlayerRaceDefinition player)
            => Verify(script, player, p => p.Name == player.Name, null, null);

        private static void Verify(string script, ItemVariant item)
            => Verify(script, item, i => i.Name == item.Name, c => c.SimpleProperties, c => c.ValuedProperties);

        private static void Verify(string script, ItemGroup item)
            => Verify(script, item, i => i.Name == item.Name, null, null);

        private static void Verify(string script, BranchDefinition branch)
            => Verify(script, branch, b => b.Name == branch.Name, null, null);

        private static void Verify(string script, MapFragment fragment)
            => Verify(script, fragment, f => f.Name == fragment.Name && VerifyNoUnicode(fragment), null, null);

        private static void Verify(string script, ConnectingMapFragment fragment)
            => Verify(script, fragment, f => f.Name == fragment.Name && VerifyNoUnicode(fragment), null, null);

        private static void Verify(string script, DefiningMapFragment fragment)
            => Verify(script, fragment, f => f.Name == fragment.Name && VerifyNoUnicode(fragment), null, null);

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

        private static void Verify<T>(string script, T variant, Func<T, bool> isValid,
            Func<T, ISet<string>> getSimpleProperties,
            Func<T, IDictionary<string, object>> getValuedProperties)
        {
            try
            {
                var serializedVariant = script == null ? variant : CSScriptLoaderBase.Load<T>(script);
                if (!isValid(serializedVariant))
                {
                    Console.WriteLine(script);
                }

                var simpleProperties = getSimpleProperties?.Invoke(serializedVariant);
                if (simpleProperties != null)
                {
                    foreach (var simpleProperty in simpleProperties)
                    {
                        var description = PropertyDescription.Loader.Get(simpleProperty);
                        if (description == null)
                        {
                            throw new InvalidOperationException("Invalid simple property: " + simpleProperty);
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
                        var description = PropertyDescription.Loader.Get(valuedProperty.Key);
                        if (description == null)
                        {
                            throw new InvalidOperationException("Invalid valued property: " + valuedProperty);
                        }
                        if (description.PropertyType == typeof(bool))
                        {
                            throw new InvalidOperationException("Simple property used as valued: " + valuedProperty);
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
    }
}