using CSharpScriptSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class StealItem : AbilityEffect, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<StealItem>();
        public ExpressionSyntax GetCreation() => Serializer.GetCreation(this);
    }
}