namespace UnicornHack.Effects
{
    public class Engulf : Effect
    {
        public Engulf()
        {
        }

        public Engulf(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Engulf(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.AppliedEffects.Add(new Engulfed(abilityContext) {Duration = Duration});
        }
    }
}