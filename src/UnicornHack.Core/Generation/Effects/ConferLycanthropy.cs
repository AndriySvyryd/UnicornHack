using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class ConferLycanthropy : Effect
    {
        public string VariantName { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            effect.EffectType = EffectType.ConferLycanthropy;
            effect.TargetName = VariantName;
        }
    }
}
