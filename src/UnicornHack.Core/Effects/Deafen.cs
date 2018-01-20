using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Deafen : DurationEffect
    {
        public Deafen()
        {
        }

        public Deafen(Game game) : base(game)
        {
        }

        public Deafen(Deafen effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Deafen(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Deafened(abilityContext, TargetActivator) {Duration = Duration});
        }
    }
}