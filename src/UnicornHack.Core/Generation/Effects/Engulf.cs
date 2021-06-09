using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Engulf : DurationEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Engulf;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Engulf>(GetPropertyConditions<Engulf>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
