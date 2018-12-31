using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public abstract class Effect
    {
        public bool TargetActivator { get; set; }

        public EffectComponent AddToAbility(AbilityComponent ability, GameManager manager)
        {
            var continuous = (ability.Activation & ActivationType.Continuous) != 0;
            using (var effectEntityReference = manager.CreateEntity())
            {
                var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                effect.ContainingAbilityId = ability.EntityId;
                effect.ShouldTargetActivator = TargetActivator;

                effectEntityReference.Referenced.Effect = effect;

                ConfigureEffect(effect);

                if (continuous
                    && effect.Duration == EffectDuration.Instant)
                {
                    effect.Duration = EffectDuration.Infinite;
                }

                return effect;
            }
        }

        protected abstract void ConfigureEffect(EffectComponent effect);
    }
}
