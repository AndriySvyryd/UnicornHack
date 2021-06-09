using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class DisableItem : Effect
    {
        protected override void ConfigureEffect(EffectComponent effect) => effect.EffectType = EffectType.DisableItem;

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<DisableItem>(GetPropertyConditions<DisableItem>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
