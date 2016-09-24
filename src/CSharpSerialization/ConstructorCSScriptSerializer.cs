using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSerialization
{
    public class ConstructorCSScriptSerializer<T> : CSScriptSerializer
    {
        private ObjectCreationExpressionSyntax _objectCreationExpression;
        private readonly IReadOnlyCollection<Func<T, object>> _parameterGetters;

        public ConstructorCSScriptSerializer()
            : this(parameterGetters: null)
        {
        }

        public ConstructorCSScriptSerializer(IReadOnlyCollection<Func<object, object>> nonGenericParameterGetters)
            : this(nonGenericParameterGetters
                .Select<Func<object, object>, Func<T, object>>(g => o => g(o)).ToArray())
        {
        }

        public ConstructorCSScriptSerializer(IReadOnlyCollection<Func<T, object>> parameterGetters)
            : base(typeof(T))
        {
            _parameterGetters = parameterGetters;
        }

        protected virtual bool GenerateEmptyArgumentList => true;

        public override ExpressionSyntax GetCreation(object obj) => GetObjectCreationExpression((T)obj);

        protected virtual ObjectCreationExpressionSyntax GetObjectCreationExpression(T obj)
            => _parameterGetters == null
               || _parameterGetters.Count == 0
                ? GenerateEmptyArgumentList
                    ? GetObjectCreationExpression().WithArgumentList(SyntaxFactory.ArgumentList())
                    : GetObjectCreationExpression()
                : GetObjectCreationExpression()
                    .WithArgumentList(AddNewLine(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                            ToCommaSeparatedList(
                                _parameterGetters.Select(a =>
                                    SyntaxFactory.Argument(GetCreationExpression(a(obj)))))))));

        protected virtual ObjectCreationExpressionSyntax GetObjectCreationExpression()
            => _objectCreationExpression ??
               (_objectCreationExpression = SyntaxFactory.ObjectCreationExpression(GetTypeSyntax(Type)));
    }
}