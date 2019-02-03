using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Activate : DurationEffect
    {
        public string Projectile { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Activate;

            if (Projectile != null)
            {
                var item = Item.Loader.Get(Projectile).AddToEntity(effect.Entity);
                effect.TargetEntityId = item.EntityId;
            }
        }
    }
}
