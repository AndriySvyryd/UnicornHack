namespace UnicornHack.Effects
{
    public class Disarm : Effect
    {
        public Disarm()
        {
        }

        public Disarm(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Disarm(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.AppliedEffects.Add(new Disarmed(abilityContext));
        }
    }
}