using CSharpScriptSerialization;

namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class Disarm : AbilityEffect, ICSScriptSerializable
    {
        private static readonly CSScriptSerializer Serializer = new ConstructorCSScriptSerializer<Disarm>();
        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}