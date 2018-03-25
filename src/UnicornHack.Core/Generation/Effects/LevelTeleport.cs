using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class LevelTeleport : Effect
    {
        protected override void ConfigureEffect(EffectComponent effect) => effect.EffectType = EffectType.LevelTeleport;
    }
}
