using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation
{
    public class Affectable
    {
        protected void CreatePropertyEffect(string property, int? value, int abilityId, GameManager manager)
        {
            if (value == null)
            {
                return;
            }

            using (var effectEntityReference = manager.CreateEntity())
            {
                var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                effect.EffectType = EffectType.ChangeProperty;
                effect.DurationTicks = (int)EffectDuration.Infinite;
                effect.PropertyName = property;
                effect.Amount = value;
                effect.Function = ValueCombinationFunction.MeanRoundDown;
                effect.ContainingAbilityId = abilityId;

                effectEntityReference.Referenced.Effect = effect;
            }
        }
    }
}
