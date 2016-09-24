using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace CSharpSerialization
{
    public abstract class CSScriptSerializer : ICSScriptSerializer
    {
        protected CSScriptSerializer(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        public abstract ExpressionSyntax GetCreation(object obj);

        public static readonly List<ICSScriptSerializerFactory> AdditionalSerializerFactories =
            new List<ICSScriptSerializerFactory>();

        public static string Serialize(object obj)
        {
            using (var workspace = new AdhocWorkspace())
            {
                return Formatter.Format(
                    GetCompilationUnitExpression(obj),
                    workspace,
                    workspace.Options)
                    .ToFullString();
            }
        }

        public static CompilationUnitSyntax GetCompilationUnitExpression(object obj)
            => SyntaxFactory.CompilationUnit()
                .WithMembers(
                    SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        SyntaxFactory.GlobalStatement(
                            SyntaxFactory.ExpressionStatement(GetCreationExpression(obj))
                                .WithSemicolonToken(SyntaxFactory.MissingToken(SyntaxKind.SemicolonToken)))));

        public static ExpressionSyntax GetCreationExpression(object obj)
        {
            var serializable = obj as ICSScriptSerializable;
            return serializable != null
                ? serializable.GetCreation()
                : GetSerializer(obj).GetCreation(obj);
        }

        private static readonly ConcurrentDictionary<Type, ICSScriptSerializer> CachedSerializers =
            new ConcurrentDictionary<Type, ICSScriptSerializer>();

        private static ICSScriptSerializer GetSerializer(object obj)
        {
            if (obj == null)
            {
                return NullCSScriptSerializer.Instance;
            }

            var type = UnwrapNullableType(obj.GetType());
            if (type == typeof(bool))
            {
                return (bool)obj
                    ? TrueCSScriptSerializer.Instance
                    : (CSScriptSerializer)FalseCSScriptSerializer.Instance;
            }

            return CachedSerializers.GetOrAdd(type, CreateSerializer);
        }

        private static ICSScriptSerializer CreateSerializer(Type type)
        {
            if (type == typeof(string))
            {
                return new LiteralCSScriptSerializer(type, SyntaxKind.StringLiteralExpression);
            }

            if (type == typeof(char))
            {
                return new LiteralCSScriptSerializer(type, SyntaxKind.CharacterLiteralExpression);
            }

            if (type == typeof(decimal)
                || type == typeof(double)
                || type == typeof(float)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(uint)
                || type == typeof(ulong)
                || type == typeof(short)
                || type == typeof(byte)
                || type == typeof(ushort)
                || type == typeof(sbyte))
            {
                return new LiteralCSScriptSerializer(type, SyntaxKind.NumericLiteralExpression);
            }

            if (type.GetTypeInfo().IsEnum)
            {
                return new EnumCSScriptSerializer(type);
            }

            if (type == typeof(Guid))
            {
                return new ConstructorCSScriptSerializer<Guid>(
                    new Func<Guid, object>[] {g => g.ToString()});
            }

            if (type == typeof(DateTime))
            {
                return new ConstructorCSScriptSerializer<DateTime>(
                    new Func<DateTime, object>[] {d => d.Ticks, d => d.Kind});
            }

            if (type == typeof(DateTimeOffset))
            {
                return new ConstructorCSScriptSerializer<DateTimeOffset>(
                    new Func<DateTimeOffset, object>[] {d => d.DateTime, d => d.Offset});
            }

            if (type == typeof(TimeSpan))
            {
                return new ConstructorCSScriptSerializer<TimeSpan>(
                    new Func<TimeSpan, object>[] {t => t.Ticks});
            }

            if (type.IsArray)
            {
                return new ArrayCSScriptSerializer(type);
            }

            if (type.IsConstructedGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();
                if (genericDefinition == typeof(Tuple<>))
                {
                    return CreateConstructorCSScriptSerializer(type, CreateTupleGetters(type, arity: 1));
                }
                if (genericDefinition == typeof(Tuple<,>))
                {
                    return CreateConstructorCSScriptSerializer(type, CreateTupleGetters(type, arity: 2));
                }
                if (genericDefinition == typeof(Tuple<,,>))
                {
                    return CreateConstructorCSScriptSerializer(type, CreateTupleGetters(type, arity: 3));
                }
                if (genericDefinition == typeof(Tuple<,,,>))
                {
                    return CreateConstructorCSScriptSerializer(type, CreateTupleGetters(type, arity: 4));
                }
                if (genericDefinition == typeof(Tuple<,,,,>))
                {
                    return CreateConstructorCSScriptSerializer(type, CreateTupleGetters(type, arity: 5));
                }
                if (genericDefinition == typeof(Tuple<,,,,,>))
                {
                    return CreateConstructorCSScriptSerializer(type, CreateTupleGetters(type, arity: 6));
                }
                if (genericDefinition == typeof(Tuple<,,,,,,>))
                {
                    return CreateConstructorCSScriptSerializer(type, CreateTupleGetters(type, arity: 7));
                }
                if (genericDefinition == typeof(Tuple<,,,,,,,>))
                {
                    return CreateConstructorCSScriptSerializer(type, CreateTupleGetters(type, arity: 8));
                }
            }

            foreach (var additionalSerializerFactory in AdditionalSerializerFactories)
            {
                var serializer = additionalSerializerFactory.TryCreate(type);
                if (serializer != null)
                {
                    return serializer;
                }
            }

            if (!IsConstructable(type))
            {
                throw new InvalidOperationException($"The type {type} does not have a public parameterless constructor");
            }

            var typeInfo = type.GetTypeInfo();
            if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return CreateCollectionCSScriptSerializer(
                    type,
                    new Func<object, object>[] {p => ((DictionaryEntry)p).Key, p => ((DictionaryEntry)p).Value},
                    o => ToEnumerable(((IDictionary)o).GetEnumerator()));
            }

            if (typeof(ICollection).GetTypeInfo().IsAssignableFrom(typeInfo)
                || TryGetElementType(type, typeof(ICollection<>)) != null)
            {
                return (CSScriptSerializer)GetDeclaredConstructor(
                    typeof(CollectionCSScriptSerializer<>).MakeGenericType(type), types: null)
                    .Invoke(parameters: null);
            }

            if (!IsInitializable(type))
            {
                throw new InvalidOperationException($"The type {type} does not have public writable properties");
            }

            // TODO: record types being constructed to avoid recursion
            return (CSScriptSerializer)GetDeclaredConstructor(
                typeof(PropertyCSScriptSerializer<>).MakeGenericType(type), types: null)
                .Invoke(parameters: null);
        }

        private static IEnumerable<object> ToEnumerable(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        private static CSScriptSerializer CreateConstructorCSScriptSerializer(
            Type type,
            IReadOnlyCollection<Func<object, object>> parameterGetters)
            => (CSScriptSerializer)GetDeclaredConstructor(
                typeof(ConstructorCSScriptSerializer<>).MakeGenericType(type),
                new[] {typeof(IReadOnlyCollection<Func<object, object>>)})
                .Invoke(new object[] {parameterGetters});

        private static CSScriptSerializer CreateCollectionCSScriptSerializer(
            Type type,
            IReadOnlyCollection<Func<object, object>> elementDecomposers,
            Func<object, IEnumerable<object>> getEnumerable)
            => (CSScriptSerializer)GetDeclaredConstructor(
                typeof(CollectionCSScriptSerializer<>).MakeGenericType(type),
                new[] {typeof(IReadOnlyCollection<Func<object, object>>), typeof(Func<object, IEnumerable<object>>)})
                .Invoke(new object[] {elementDecomposers, getEnumerable});

        private static Func<object, object>[] CreateTupleGetters(Type type, int arity)
        {
            var getters = new List<Func<object, object>>();
            for (var i = 1; i <= arity; i++)
            {
                var itemProperty = type.GetTypeInfo().GetProperty("Item" + i);
                getters.Add(o => itemProperty.GetValue(o));
            }
            return getters.ToArray();
        }

        protected static bool IsConstructable(Type type)
            => !type.GetTypeInfo().IsInterface
               && !type.GetTypeInfo().IsAbstract
               && !type.GetTypeInfo().IsGenericTypeDefinition
               && GetDeclaredConstructor(type, types: null) != null;

        protected static bool IsInitializable(Type type) => type.GetRuntimeProperties().Where(IsCandidateProperty).Any();

        protected static bool IsCandidateProperty(PropertyInfo property)
            => !(property.GetMethod ?? property.SetMethod).IsStatic
               && property.GetIndexParameters().Length == 0
               && property.CanRead
               && property.CanWrite
               && property.GetMethod != null
               && property.GetMethod.IsPublic
               && property.SetMethod != null
               && property.SetMethod.IsPublic;

        protected static Type UnwrapNullableType(Type type) => Nullable.GetUnderlyingType(type) ?? type;

        protected static Type TryGetElementType(Type type, Type interfaceOrBaseType)
        {
            var types = GetGenericTypeImplementations(type, interfaceOrBaseType).ToList();
            return types.Count == 1 ? types[index: 0].GetTypeInfo().GenericTypeArguments.FirstOrDefault() : null;
        }

        private static readonly ConcurrentDictionary<Type, object> TypeDefaults =
            new ConcurrentDictionary<Type, object>();

        protected static object GetDefault(Type type)
            => type.GetTypeInfo().IsValueType ? TypeDefaults.GetOrAdd(type, Activator.CreateInstance) : null;

        protected static bool IsDefault(object obj)
            => obj == null || obj.Equals(GetDefault(obj.GetType()));

        protected static IEnumerable<Type> GetGenericTypeImplementations(Type type, Type interfaceOrBaseType)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                return (interfaceOrBaseType.GetTypeInfo().IsInterface
                    ? typeInfo.ImplementedInterfaces
                    : GetBaseTypes(type))
                    .Union(new[] {type})
                    .Where(t => t.GetTypeInfo().IsGenericType
                                && (t.GetGenericTypeDefinition() == interfaceOrBaseType));
            }

            return Enumerable.Empty<Type>();
        }

        protected static IEnumerable<Type> GetBaseTypes(Type type)
        {
            type = type.GetTypeInfo().BaseType;
            while (type != null)
            {
                yield return type;

                type = type.GetTypeInfo().BaseType;
            }
        }

        protected static ConstructorInfo GetDeclaredConstructor(Type type, Type[] types)
            => type.GetTypeInfo().DeclaredConstructors
                .SingleOrDefault(
                    c => !c.IsStatic
                         && c.GetParameters().Where(p => !p.ParameterType.IsGenericParameter)
                             .Select(p => p.ParameterType).SequenceEqual(types ?? new Type[0]));

        protected static IEnumerable<SyntaxNodeOrToken> ToCommaSeparatedList(IEnumerable<CSharpSyntaxNode> tokens)
            => tokens.Aggregate(new List<SyntaxNodeOrToken>(),
                (list, current) =>
                {
                    if (list.Count > 0)
                    {
                        list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                    }
                    list.Add(current);
                    return list;
                });

        protected static TypeSyntax GetTypeSyntax(Type type)
        {
            if (type == typeof(bool))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));
            }
            if (type == typeof(byte))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword));
            }
            if (type == typeof(sbyte))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.SByteKeyword));
            }
            if (type == typeof(char))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.CharKeyword));
            }
            if (type == typeof(short))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ShortKeyword));
            }
            if (type == typeof(ushort))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UShortKeyword));
            }
            if (type == typeof(int))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
            }
            if (type == typeof(uint))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword));
            }
            if (type == typeof(long))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword));
            }
            if (type == typeof(ulong))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ULongKeyword));
            }
            if (type == typeof(float))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword));
            }
            if (type == typeof(double))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
            }
            if (type == typeof(decimal))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DecimalKeyword));
            }
            if (type == typeof(string))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return SyntaxFactory.NullableType(GetTypeSyntax(underlyingType));
            }
            if (type.IsArray)
            {
                return SyntaxFactory.ArrayType(GetTypeSyntax(GetArrayElementType(type)))
                    .WithRankSpecifiers(SyntaxFactory.List(
                        GetArrayRanks(type)));
            }

            var genericArgs = type.GetTypeInfo().GetGenericArguments();
            return genericArgs.Length > 0
                ? SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier(type.Name.Substring(startIndex: 0,
                        length: type.Name.IndexOf(value: '`'))))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList<TypeSyntax>(
                                ToCommaSeparatedList(genericArgs.Select(GetTypeSyntax)))))
                : (TypeSyntax)SyntaxFactory.IdentifierName(type.Name);
        }

        private static Type GetArrayElementType(Type type)
            => type.IsArray ? GetArrayElementType(type.GetElementType()) : type;

        private static IEnumerable<ArrayRankSpecifierSyntax> GetArrayRanks(Type type)
            => type == null || !type.IsArray
                ? Enumerable.Empty<ArrayRankSpecifierSyntax>()
                : Enumerable.Repeat(SyntaxFactory.ArrayRankSpecifier(
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(
                        ToCommaSeparatedList(
                            Enumerable.Repeat(SyntaxFactory.OmittedArraySizeExpression(), type.GetArrayRank())))),
                    count: 1)
                    .Concat(GetArrayRanks(type.GetElementType()));

        protected static TSyntax AddNewLine<TSyntax>(TSyntax expression)
            where TSyntax : SyntaxNode
            => expression.FullSpan.Length > 120
                ? expression.WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed)
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)
                : expression;
    }
}