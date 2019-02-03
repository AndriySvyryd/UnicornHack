using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class AddAbility : DurationEffect
    {
        public Ability Ability { get; set; }
        public string AbilityName { get; set; }
        public int Level { get; set; }
        public ValueCombinationFunction Function { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.AddAbility;

            if (Ability != null)
            {
                Ability.AddToEffect(effect.Entity);
            }
            else
            {
                effect.TargetName = AbilityName;
                effect.Amount = Level;
                effect.Function = Function;
            }
        }
    }
}
