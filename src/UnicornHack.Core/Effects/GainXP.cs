using UnicornHack.Abilities;

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

        public GainXP(GainXP effect, Game game)
            : base(effect, game)
            => Amount = effect.Amount;

        public int Amount { get; set; }

        public override Effect Copy(Game game) => new GainXP(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            if ((TargetActivator ? abilityContext.Activator : abilityContext.TargetEntity) is Player player)
            {
                player.AddXP(Amount);
                abilityContext.Add(new GainedXP(abilityContext, TargetActivator) {Amount = Amount});
            }
        }
    }
}