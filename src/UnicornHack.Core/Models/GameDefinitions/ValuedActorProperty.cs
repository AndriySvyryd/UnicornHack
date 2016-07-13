namespace UnicornHack.Models.GameDefinitions
{
    public class ValuedActorProperty<T> : ActorProperty
    {
        public readonly ValuedActorPropertyType Type;
        public readonly T Value;
        public readonly bool Relative;

        public ValuedActorProperty(ValuedActorPropertyType type, T value, bool granted, bool relative)
            : base(granted)
        {
            // TODO: Assert T corresponds to type
            Type = type;
            Value = value;
            Relative = relative;
        }
    }
}