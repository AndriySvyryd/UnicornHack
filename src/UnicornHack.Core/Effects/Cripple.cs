namespace UnicornHack.Effects
{
    public class Cripple : Effect
    {
        public Cripple()
        {
        }

        public Cripple(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Cripple(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.AppliedEffects.Add(new Confused(abilityContext) {Duration = Duration});
        }
    }
}