using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSerialization
{
    public interface ICSScriptSerializable
    {
        ExpressionSyntax GetCreation();
    }
}