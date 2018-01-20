using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class ChangeProperty<T> : DurationEffect
    {
        public ChangeProperty()
        {
        }

        public ChangeProperty(Game game) : base(game)
        {
        }

        public ChangeProperty(ChangeProperty<T> effect, Game game)
            : base(effect, game)
        {
            PropertyName = effect.PropertyName;
            Value = effect.Value;
            Function = effect.Function;
        }

        public string PropertyName { get; set; }
        public T Value { get; set; }
        public ValueCombinationFunction Function { get; set; }

        public override Effect Copy(Game game) => new ChangeProperty<T>(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            var newEffect = ChangedProperty.Create<T>(abilityContext, TargetActivator);
            newEffect.Duration = Duration;
            newEffect.PropertyName = PropertyName;
            newEffect.Function = Function;
            newEffect.Value = Value;

            newEffect.Add();
            newEffect.UpdateProperty();
        }
    }
}