using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Sedate : DurationEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Sedate;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Sedate>(GetPropertyConditions<Sedate>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
