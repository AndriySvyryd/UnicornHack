using CSharpSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class Suffocate : AbilityEffect, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<Suffocate>();
        public ExpressionSyntax GetCreation() => Serializer.GetCreation(this);
    }
}