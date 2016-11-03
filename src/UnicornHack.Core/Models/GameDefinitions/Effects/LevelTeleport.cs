using CSharpScriptSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class LevelTeleport : AbilityEffect, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<LevelTeleport>();
        public ExpressionSyntax GetCreation() => Serializer.GetCreation(this);
    }
}