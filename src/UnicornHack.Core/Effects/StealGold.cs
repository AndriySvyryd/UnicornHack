using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class StealGold : Effect
    {
        public StealGold()
        {
        }

        public StealGold(Game game) : base(game)
        {
        }

        public StealGold(StealGold effect, Game game)
            : base(effect, game)
            => Amount = effect.Amount;

        public int Amount { get; set; }

        public override Effect Copy(Game game) => new StealGold(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new GoldStolen(abilityContext, TargetActivator) {Amount = Amount});
        }
    }
}