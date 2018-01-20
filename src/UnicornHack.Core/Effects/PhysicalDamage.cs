using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class PhysicalDamage : DamageEffect
    {
        public PhysicalDamage()
        {
        }

        public PhysicalDamage(Game game) : base(game)
        {
        }

        public PhysicalDamage(PhysicalDamage effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new PhysicalDamage(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded
                || Damage == 0)
            {
                return;
            }

            var damage = ApplyDamage(
                abilityContext,
                PropertyData.PhysicalAbsorption.Name,
                PropertyData.PhysicalResistance.Name);

            abilityContext.Add(new PhysicallyDamaged(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}