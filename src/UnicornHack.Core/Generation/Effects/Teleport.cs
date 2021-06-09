using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Teleport : Effect
    {
        protected override void ConfigureEffect(EffectComponent effect)
            => effect.EffectType = EffectType.Teleport;

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Teleport>(GetPropertyConditions<Teleport>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
