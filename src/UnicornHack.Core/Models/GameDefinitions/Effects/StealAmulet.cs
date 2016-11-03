using CSharpScriptSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class StealAmulet : AbilityEffect, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<StealAmulet>();
        public ExpressionSyntax GetCreation() => Serializer.GetCreation(this);
    }
}