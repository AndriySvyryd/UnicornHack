using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class ScriptedEffect : Effect
    {
        public string Script { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            effect.EffectType = EffectType.Heal;
            effect.Amount = 0;
        }
    }
}
