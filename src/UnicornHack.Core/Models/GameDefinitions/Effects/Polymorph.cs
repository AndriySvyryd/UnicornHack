using CSharpSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class Polymorph : AbilityEffect, ICSScriptSerializable
    {
        // Polymorphs items
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<Polymorph>();
        public ExpressionSyntax GetCreation() => Serializer.GetCreation(this);
    }
}