using CSharpScriptSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class Stone : AbilityEffect, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<Stone>();
        public ExpressionSyntax GetCreation() => Serializer.GetCreation(this);
    }
}