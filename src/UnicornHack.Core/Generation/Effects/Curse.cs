using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Curse : DurationEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Curse;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Curse>(GetPropertyConditions<Curse>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
