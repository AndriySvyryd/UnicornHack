using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Engulf : DurationEffect
    {
        public Engulf()
        {
        }

        public Engulf(Game game) : base(game)
        {
        }

        public Engulf(Engulf effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Engulf(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Engulfed(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}