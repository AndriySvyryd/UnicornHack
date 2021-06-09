using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class RemoveItem : Effect
    {
        protected override void ConfigureEffect(EffectComponent effect)
            => effect.EffectType = EffectType.RemoveItem;

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<RemoveItem>(GetPropertyConditions<RemoveItem>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
