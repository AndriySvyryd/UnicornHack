using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class PsychicDamage : DamageEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.PsychicDamage;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<PsychicDamage>(GetPropertyConditions<PsychicDamage>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
