using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class Freeze : DamageEffect
    {
        public Freeze()
        {
        }

        public Freeze(Game game) : base(game)
        {
        }

        public Freeze(Freeze effect, Game game)
            : base(effect, game)
        {
        }

        public override Effect Copy(Game game) => new Freeze(this, game);

        // TODO: Freezes items
        // TODO: Slows, removes burning, dissolving
        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (!abilityContext.Succeeded
                || Damage == 0)
            {
                return;
            }

            var damage = ApplyDamage(
                abilityContext,
                null,
                PropertyData.ColdResistance.Name);

            abilityContext.Add(new Frozen(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}