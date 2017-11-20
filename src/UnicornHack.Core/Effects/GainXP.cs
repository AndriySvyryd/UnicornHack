namespace UnicornHack.Effects
{
    public class GainXP : Effect
    {
        public GainXP()
        {
        }

        public GainXP(Game game) : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Copy(Game game) => new GainXP(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            if (abilityContext.Target is Player player)
            {
                player.AddXP(Amount);
                abilityContext.Add(new GainedXP(abilityContext) { Amount = Amount });
            }
        }
    }
}