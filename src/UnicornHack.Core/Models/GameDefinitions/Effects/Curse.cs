using CSharpScriptSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class Curse : AbilityEffect, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<Curse>();
        public ExpressionSyntax GetCreation() => Serializer.GetCreation(this);
    }
}