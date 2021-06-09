using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation
{
    public class Affectable
    {
        protected void CreatePropertyEffect(
            string propertyName, int? value, int abilityId, GameManager manager, ValueCombinationFunction? function = null)
        {
            if (value == null)
            {
                return;
            }

            using (var effectEntityReference = manager.CreateEntity())
            {
                var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                effect.EffectType = EffectType.ChangeProperty;
                effect.Duration = EffectDuration.Infinite;
                effect.TargetName = propertyName;
                effect.AppliedAmount = value;
                effect.CombinationFunction = function ?? ValueCombinationFunction.Sum;
                effect.ContainingAbilityId = abilityId;

                effectEntityReference.Referenced.Effect = effect;
            }
        }
    }
}
