using CSharpSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class StealGold : AbilityEffect, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<StealGold>();
        public ExpressionSyntax GetCreation() => Serializer.GetCreation(this);
    }
}