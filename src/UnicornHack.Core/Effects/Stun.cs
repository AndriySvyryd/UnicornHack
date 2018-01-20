using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Stun : DurationEffect
    {
        public Stun()
        {
        }

        public Stun(Game game) : base(game)
        {
        }

        public Stun(Stun effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Stun(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Stunned(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}