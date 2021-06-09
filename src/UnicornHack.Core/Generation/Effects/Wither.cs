using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Wither : DamageEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Wither;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Wither>(GetPropertyConditions<Wither>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
