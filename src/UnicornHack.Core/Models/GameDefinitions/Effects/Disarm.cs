using CSharpSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class Disarm : AbilityEffect, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<Disarm>();
        public ExpressionSyntax GetCreation() => Serializer.GetCreation(this);
    }
}