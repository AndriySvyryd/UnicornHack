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

        public int Amount { get; set; }

        public override Effect Copy(Game game) => new Heal(game) {Amount = Amount};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            if (abilityContext.Target is Actor actor)
            {
                actor.ChangeCurrentHP(Amount);
                abilityContext.Add(new Healed(abilityContext) { Amount = Amount });
            }
        }
    }
}