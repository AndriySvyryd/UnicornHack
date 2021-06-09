using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Shock : DamageEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Shock;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Shock>(GetPropertyConditions<Shock>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
