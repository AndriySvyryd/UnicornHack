using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSerialization
{
    public class LiteralCSScriptSerializer : CSScriptSerializer
    {
        public LiteralCSScriptSerializer(Type type, SyntaxKind kind)
            : base(type)
        {
            Kind = kind;
        }

        protected SyntaxKind Kind { get; }

        public override ExpressionSyntax GetCreation(object obj)
            => SyntaxFactory.LiteralExpression(
                Kind,
                SyntaxFactory.Literal((dynamic)obj));
    }
}