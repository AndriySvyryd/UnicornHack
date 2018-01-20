using UnicornHack.Abilities;

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

        public DrainEnergy(DrainEnergy effect, Game game)
            : base(effect, game)
            => Amount = effect.Amount;

        public int Amount { get; set; }

        public override Effect Copy(Game game) => new DrainEnergy(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            (abilityContext.TargetEntity as Actor)?.ChangeCurrentEP(-1 * Amount);
            (abilityContext.Activator as Actor)?.ChangeCurrentEP(Amount);
            abilityContext.Add(new EnergyDrained(abilityContext, TargetActivator) {Amount = Amount});
        }
    }
}