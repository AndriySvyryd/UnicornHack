using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSerialization
{
    public interface ICSScriptSerializer
    {
        ExpressionSyntax GetCreation(object obj);
    }
}