using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class GainXP : AmountEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.GainXP;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<GainXP>(GetPropertyConditions<GainXP>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
