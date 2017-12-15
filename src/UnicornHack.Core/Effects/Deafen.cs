using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Deafen : Effect
    {
        public Deafen()
        {
        }

        public Deafen(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Deafen(game) {Duration = Duration};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Deafened(abilityContext) {Duration = Duration});
        }
    }
}