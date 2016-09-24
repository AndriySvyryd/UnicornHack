using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSerialization
{
    public class TrueCSScriptSerializer : CSScriptSerializer
    {
        public static readonly TrueCSScriptSerializer Instance = new TrueCSScriptSerializer();

        private TrueCSScriptSerializer()
            : base(typeof(bool))
        {
        }

        public override ExpressionSyntax GetCreation(object obj)
            => SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
    }
}