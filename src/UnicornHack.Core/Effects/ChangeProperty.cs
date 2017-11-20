namespace UnicornHack.Effects
{
    public class ChangeProperty<T> : Effect
    {
        public ChangeProperty()
        {
        }

        public ChangeProperty(Game game) : base(game)
        {
        }

        public string PropertyName { get; set; }
        public T Value { get; set; }
        public ValueCombinationFunction Function { get; set; }

        public override Effect Copy(Game game) => new ChangeProperty<T>(game)
        {
            PropertyName = PropertyName,
            Value = Value,
            Function = Function,
            Duration = Duration
        };

        public override void Apply(AbilityActivationContext abilityContext)
        {
            var newEffect = ChangedProperty.Create<T>(abilityContext);
            newEffect.Duration = Duration;
            newEffect.PropertyName = PropertyName;
            newEffect.Function = Function;
            newEffect.Value = Value;

            newEffect.Add();
            newEffect.UpdateProperty();
        }
    }
}