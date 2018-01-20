using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Slow : DurationEffect
    {
        public Slow()
        {
        }

        public Slow(Game game) : base(game)
        {
        }

        public Slow(Slow effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Slow(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Slowed(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}