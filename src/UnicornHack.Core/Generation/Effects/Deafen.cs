using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class Deafen : DurationEffect
    {
        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.Deafen;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<Deafen>(GetPropertyConditions<Deafen>());

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
