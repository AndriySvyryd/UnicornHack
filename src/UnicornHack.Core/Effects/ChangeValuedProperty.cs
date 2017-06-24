using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public class ChangeValuedProperty<T> : Effect
    {
        public ChangeValuedProperty()
        {
        }

        public ChangeValuedProperty(Game game)
            : base(game)
        {
        }

        public string PropertyName { get; set; }
        public T Value { get; set; }
        public bool IsAbsolute { get; set; }
        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new ChangeValuedProperty<T>(game)
            {
                PropertyName = PropertyName,
                Value = Value,
                IsAbsolute = IsAbsolute,
                Duration = Duration
            };

        public override void Apply(AbilityActivationContext abilityContext)
        {
        }
    }
}