using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Recharge : AmountEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Recharge;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Recharge>(GetPropertyConditions<Recharge>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
