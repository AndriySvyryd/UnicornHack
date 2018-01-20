using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class MagicalDamage : DamageEffect
    {
        public MagicalDamage()
        {
        }

        public MagicalDamage(Game game) : base(game)
        {
        }

        public MagicalDamage(MagicalDamage effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new MagicalDamage(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded
                || Damage == 0)
            {
                return;
            }

            var damage = ApplyDamage(
                abilityContext,
                PropertyData.MagicAbsorption.Name,
                PropertyData.MagicResistance.Name);

            abilityContext.Add(new MagicallyDamaged(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}