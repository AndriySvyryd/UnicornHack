using System.Diagnostics;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public abstract class DurationEffect : Effect
    {
        public EffectDuration Duration { get; set; }
        public string DurationAmount { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            Debug.Assert(DurationAmount == null
                         || Duration == EffectDuration.UntilTimeout
                         || Duration == EffectDuration.UntilXPGained);

            effect.Duration = Duration;
            effect.DurationAmount = DurationAmount;
        }
    }
}
