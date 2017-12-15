using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Stun : Effect
    {
        public Stun()
        {
        }

        public Stun(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Stun(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Stunned(abilityContext) {Duration = Duration});
        }
    }
}