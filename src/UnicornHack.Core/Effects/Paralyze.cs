using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Paralyze : DurationEffect
    {
        public Paralyze()
        {
        }

        public Paralyze(Game game) : base(game)
        {
        }

        public Paralyze(Paralyze effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Paralyze(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Paralyzed(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}