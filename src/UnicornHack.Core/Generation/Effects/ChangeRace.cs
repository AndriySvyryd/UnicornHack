using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class ChangeRace : DurationEffect
    {
        public string RaceName { get; set; }
        public bool Remove { get; set; }
        public int Delay { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.ChangeRace;
            effect.TargetName = RaceName;
            effect.Amount = Remove ? -1 : 1;
        }
    }
}
