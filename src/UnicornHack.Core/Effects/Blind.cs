using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Blind : DurationEffect
    {
        public Blind()
        {
        }

        public Blind(Game game) : base(game)
        {
        }

        public Blind(Blind effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Blind(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Blinded(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}