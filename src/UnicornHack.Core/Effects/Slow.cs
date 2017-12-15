using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Slow : Effect
    {
        public Slow()
        {
        }

        public Slow(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Slow(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Slowed(abilityContext) {Duration = Duration});
        }
    }
}