using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class Envenom : DamageEffect
    {
        public Envenom()
        {
        }

        public Envenom(Game game) : base(game)
        {
        }

        public Envenom(Envenom effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Envenom(this, game);

        // TODO: Decays items
        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded)
            {
                return;
            }

            if (!abilityContext.Succeeded
                || Damage == 0)
            {
                return;
            }

            var damage = ApplyDamage(
                abilityContext,
                null,
                PropertyData.VenomResistance.Name);

            abilityContext.Add(new Envenomed(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}