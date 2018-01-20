using UnicornHack.Abilities;
using UnicornHack.Data.Properties;

namespace UnicornHack.Effects
{
    public class Disintegrate : DamageEffect
    {
        public Disintegrate()
        {
        }

        public Disintegrate(Game game) : base(game)
        {
        }

        public Disintegrate(Disintegrate effect, Game game)
            : base(effect, game)
        {
        }

        // TODO: Withers items
        public override Effect Copy(Game game) => new Disintegrate(this, game);

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
                PropertyData.DisintegrationResistance.Name);

            abilityContext.Add(new Disintegrated(abilityContext, TargetActivator) {Damage = damage});
        }
    }
}