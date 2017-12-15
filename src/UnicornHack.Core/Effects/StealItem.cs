using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class StealItem : Effect
    {
        public StealItem()
        {
        }

        public StealItem(Game game) : base(game)
        {
        }

        public override Effect Copy(Game game) => new StealItem(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new ItemStolen(abilityContext));
        }
    }
}