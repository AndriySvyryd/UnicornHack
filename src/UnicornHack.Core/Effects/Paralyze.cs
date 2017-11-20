namespace UnicornHack.Effects
{
    public class Paralyze : Effect
    {
        public Paralyze()
        {
        }

        public Paralyze(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Paralyze(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Paralyzed(abilityContext) {Duration = Duration});
        }
    }
}