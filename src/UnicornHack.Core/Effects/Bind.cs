namespace UnicornHack.Effects
{
    public class Bind : Effect
    {
        public Bind()
        {
        }

        public Bind(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Bind(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.AppliedEffects.Add(new Bound(abilityContext) {Duration = Duration});
        }
    }
}