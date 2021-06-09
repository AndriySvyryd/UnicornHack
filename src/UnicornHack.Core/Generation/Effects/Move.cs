using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Move : Effect
    {
        protected override void ConfigureEffect(EffectComponent effect)
            => effect.EffectType = EffectType.Move;

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Move>(GetPropertyConditions<Move>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
