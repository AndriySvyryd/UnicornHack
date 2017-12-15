using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Blind : Effect
    {
        public Blind()
        {
        }

        public Blind(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Blind(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Blinded(abilityContext) {Duration = Duration});
        }
    }
}