using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class Disintegrate : Effect
    {
        public Disintegrate()
        {
        }

        public Disintegrate(Game game) : base(game)
        {
        }

        // TODO: Withers items
        public int Damage { get; set; }

        public override Effect Copy(Game game) => new Disintegrate(game) {Damage = Damage};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            (abilityContext.Target as Actor)?.ChangeCurrentHP(-1 * Damage);
            abilityContext.Add(new Disintegrated(abilityContext) {Damage = Damage});
        }
    }
}