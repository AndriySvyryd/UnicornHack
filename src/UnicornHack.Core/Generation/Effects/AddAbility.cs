using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class AddAbility : DurationEffect
    {
        public Ability Ability { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.AddAbility;

            Ability.AddToEffect(effect.Entity);
        }
    }
}
