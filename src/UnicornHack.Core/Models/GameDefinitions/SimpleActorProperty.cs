namespace UnicornHack.Models.GameDefinitions
{
    public class SimpleActorProperty : ActorProperty
    {
        public readonly SimpleActorPropertyType Type;

        public SimpleActorProperty(SimpleActorPropertyType type, bool granted)
            : base(granted)
        {
            Type = type;
        }
    }
}