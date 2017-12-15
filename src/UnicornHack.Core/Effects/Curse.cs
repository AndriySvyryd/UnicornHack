using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Curse : Effect
    {
        public Curse()
        {
        }

        public Curse(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new Curse(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new Cursed(abilityContext) {Duration = Duration});
        }
    }
}