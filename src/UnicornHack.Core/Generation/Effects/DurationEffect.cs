using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public abstract class DurationEffect : Effect
    {
        public int Duration { get; set; }

        protected override void ConfigureEffect(EffectComponent effect) => effect.DurationTicks = Duration;
    }
}
