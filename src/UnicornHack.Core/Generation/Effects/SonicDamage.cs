using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class SonicDamage : DamageEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.SonicDamage;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<SonicDamage>(GetPropertyConditions<SonicDamage>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
