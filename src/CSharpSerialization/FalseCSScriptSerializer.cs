using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSerialization
{
    public class FalseCSScriptSerializer : CSScriptSerializer
    {
        public static readonly FalseCSScriptSerializer Instance = new FalseCSScriptSerializer();

        private FalseCSScriptSerializer()
            : base(typeof(bool))
        {
        }

        public override ExpressionSyntax GetCreation(object obj)
            => SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
    }
}