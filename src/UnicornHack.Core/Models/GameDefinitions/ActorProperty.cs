namespace UnicornHack.Models.GameDefinitions
{
    public abstract class ActorProperty
    {
        public readonly bool Granted;

        protected ActorProperty(bool granted)
        {
            Granted = granted;
        }

        public static ActorProperty Add(SimpleActorPropertyType type)
        {
            // TODO: memoize the values
            return new SimpleActorProperty(type, granted: true);
        }

        public static ActorProperty Remove(SimpleActorPropertyType type)
        {
            return new SimpleActorProperty(type, granted: false);
        }

        public static ActorProperty Add<T>(ValuedActorPropertyType type, T value)
        {
            return new ValuedActorProperty<T>(type, value, granted: true, relative: true);
        }

        public static ActorProperty Remove<T>(ValuedActorPropertyType type, T value)
        {
            return new ValuedActorProperty<T>(type, value, granted: false, relative: true);
        }

        public static ActorProperty Set<T>(ValuedActorPropertyType type, T value)
        {
            return new ValuedActorProperty<T>(type, value, granted: false, relative: false);
        }

        public static ActorProperty Unset<T>(ValuedActorPropertyType type, T value)
        {
            return new ValuedActorProperty<T>(type, value, granted: false, relative: false);
        }
    }
}