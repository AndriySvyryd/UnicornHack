using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Poison : Effect
    {
        public Poison()
        {
        }

        public Poison(Game game) : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Copy(Game game) => new Poison(game) {Damage = Damage};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            (abilityContext.TargetEntity as Actor)?.ChangeCurrentHP(-1 * Damage);
            abilityContext.Add(new Poisoned(abilityContext) {Damage = Damage});
        }
    }
}