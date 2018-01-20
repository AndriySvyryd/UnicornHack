using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Heal : Effect
    {
        public Heal()
        {
        }

        public Heal(Game game) : base(game)
        {
        }

        public Heal(Heal effect, Game game)
            : base(effect, game)
            => Amount = effect.Amount;

        public int Amount { get; set; }

        public override Effect Copy(Game game) => new Heal(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded
                || Amount == 0)
            {
                return;
            }

            if ((TargetActivator ? abilityContext.Activator : abilityContext.TargetEntity) is Actor actor)
            {
                actor.ChangeCurrentHP(Amount);
                abilityContext.Add(new Healed(abilityContext, TargetActivator) {Amount = Amount});
            }
        }
    }
}