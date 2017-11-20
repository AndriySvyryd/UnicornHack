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

        public int Amount { get; set; }

        public override Effect Copy(Game game) => new StealGold(game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Add(new GoldStolen(abilityContext) {Amount = Amount});
        }
    }
}