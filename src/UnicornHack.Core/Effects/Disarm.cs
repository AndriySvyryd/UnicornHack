using UnicornHack.Abilities;

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

        public Disarm(Disarm effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Disarm(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Disarmed(abilityContext, TargetActivator));
        }
    }
}