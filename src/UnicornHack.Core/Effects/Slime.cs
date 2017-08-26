namespace UnicornHack.Effects
{
    public class Slime : Effect
    {
        public Slime()
        {
        }

        public Slime(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Slime(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.AppliedEffects.Add(new Slimed(abilityContext) {Duration = Duration});
        }
    }
}