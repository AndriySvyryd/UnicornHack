namespace UnicornHack.Effects
{
    public class DrainEnergy : Effect
    {
        public DrainEnergy()
        {
        }

        public DrainEnergy(Game game) : base(game)
        {
        }

        public int Amount { get; set; }

        public override Effect Copy(Game game) => new DrainEnergy(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            abilityContext.Target.ChangeCurrentEP(-1 * Amount);
            abilityContext.Activator.ChangeCurrentEP(Amount);
            abilityContext.AppliedEffects.Add(new EnergyDrained(abilityContext) {Amount = Amount});
        }
    }
}