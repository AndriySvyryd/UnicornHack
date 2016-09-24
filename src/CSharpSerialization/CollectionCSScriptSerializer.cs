using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSerialization
{
    public class CollectionCSScriptSerializer<T> : ConstructorCSScriptSerializer<T>
    {
        private readonly IReadOnlyCollection<Func<object, object>> _elementDecomposers;
        private readonly Func<T, IEnumerable<object>> _getEnumerable;

        public CollectionCSScriptSerializer()
            : this(elementDecomposers: null, parameterGetters: null)
        {
        }

        public CollectionCSScriptSerializer(
            IReadOnlyCollection<Func<object, object>> elementDecomposers)
            : this(elementDecomposers, parameterGetters: null)
        {
        }

        public CollectionCSScriptSerializer(
            IReadOnlyCollection<Func<T, object>> parameterGetters)
            : this(elementDecomposers: null, parameterGetters: parameterGetters)
        {
        }

        public CollectionCSScriptSerializer(
            IReadOnlyCollection<Func<object, object>> elementDecomposers,
            Func<object, IEnumerable<object>> getEnumerable)
            : this(elementDecomposers, nonGenericParameterGetters: null, nonGenericGetEnumerable: getEnumerable)
        {
        }

        public CollectionCSScriptSerializer(
            IReadOnlyCollection<Func<T, object>> parameterGetters,
            Func<T, IEnumerable<object>> getEnumerable)
            : this(elementDecomposers: null, parameterGetters: parameterGetters, getEnumerable: getEnumerable)
        {
        }

        public CollectionCSScriptSerializer(
            IReadOnlyCollection<Func<object, object>> elementDecomposers,
            IReadOnlyCollection<Func<object, object>> nonGenericParameterGetters)
            : this(elementDecomposers, nonGenericParameterGetters, nonGenericGetEnumerable: null)
        {
        }

        public CollectionCSScriptSerializer(
            IReadOnlyCollection<Func<object, object>> elementDecomposers,
            IReadOnlyCollection<Func<T, object>> parameterGetters)
            : this(elementDecomposers, parameterGetters, getEnumerable: null)
        {
        }

        public CollectionCSScriptSerializer(
            IReadOnlyCollection<Func<object, object>> elementDecomposers,
            IReadOnlyCollection<Func<object, object>> nonGenericParameterGetters,
            Func<object, IEnumerable<object>> nonGenericGetEnumerable)
            : this(nonGenericParameterGetters)
        {
            _elementDecomposers = elementDecomposers;
            _getEnumerable = nonGenericGetEnumerable != null
                ? (Func<T, IEnumerable<object>>)(o => nonGenericGetEnumerable(o))
                : (o => ((IEnumerable)o).Cast<object>());
        }

        public CollectionCSScriptSerializer(
            IReadOnlyCollection<Func<object, object>> elementDecomposers,
            IReadOnlyCollection<Func<T, object>> parameterGetters,
            Func<T, IEnumerable<object>> getEnumerable)
            : base(parameterGetters)
        {
            _elementDecomposers = elementDecomposers;
            _getEnumerable = getEnumerable ?? (o => ((IEnumerable)o).Cast<object>());
        }

        protected override bool GenerateEmptyArgumentList => false;

        public override ExpressionSyntax GetCreation(object obj)
            => GetObjectCreationExpression((T)obj);

        protected override ObjectCreationExpressionSyntax GetObjectCreationExpression(T obj)
            => base.GetObjectCreationExpression(obj)
                .WithInitializer(AddNewLine(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(
                            ToCommaSeparatedList(_getEnumerable(obj)
                                .Select(GetElementExpression))))));

        protected virtual ExpressionSyntax GetElementExpression(object element)
            => _elementDecomposers == null
               || _elementDecomposers.Count == 0
                ? GetCreationExpression(element)
                : _elementDecomposers.Count == 1
                    ? GetCreationExpression(_elementDecomposers.First()(element))
                    : SyntaxFactory.InitializerExpression(
                        SyntaxKind.ComplexElementInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(
                            ToCommaSeparatedList(_elementDecomposers
                                .Select(getSubelement => GetCreationExpression(getSubelement(element))))));
    }
}