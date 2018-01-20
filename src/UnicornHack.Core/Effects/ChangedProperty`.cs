using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public abstract class ChangedProperty<T> : ChangedProperty
    {
        protected ChangedProperty()
        {
        }

        protected ChangedProperty(AbilityActivationContext abilityContext, bool targetActivator)
            : base(abilityContext, targetActivator)
        {
        }

        public T Value { get; set; }

        public abstract void Apply(CalculatedProperty<T> property, ref (int RunningSum, int SummandCount) state);

        public override void UpdateProperty()
        {
            Entity.InvalidateProperty<T>(PropertyName);
        }
    }
}