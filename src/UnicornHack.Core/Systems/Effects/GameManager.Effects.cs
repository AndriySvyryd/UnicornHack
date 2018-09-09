using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> Effects { get; private set; }
        public EntityRelationship<GameEntity> AppliedEffectsToAffectableEntityRelationship { get; private set; }
        public EntityRelationship<GameEntity> AppliedEffectsToSourceAbilityRelationship { get; private set; }
        public EntityRelationship<GameEntity> EffectsToContainingAbilityRelationship { get; private set; }
        public EffectApplicationSystem EffectApplicationSystem { get; private set; }

        private void InitializeEffects(SequentialMessageQueue<GameManager> queue)
        {
            Add<EffectComponent>(EntityComponent.Effect, poolSize: 32);

            Effects = CreateGroup(nameof(Effects),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Effect));

            AppliedEffectsToAffectableEntityRelationship = new EntityRelationship<GameEntity>(
                nameof(AppliedEffectsToAffectableEntityRelationship),
                Effects,
                AffectableEntities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((EffectComponent)component).AffectedEntityId,
                    (int)EntityComponent.Effect),
                (effectEntity, _, __, ___) => effectEntity.RemoveComponent((int)EntityComponent.Effect),
                referencedKeepAlive: false, referencingKeepAlive: true);

            EffectsToContainingAbilityRelationship = new EntityRelationship<GameEntity>(
                nameof(EffectsToContainingAbilityRelationship),
                Effects,
                Abilities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((EffectComponent)component).ContainingAbilityId,
                    (int)EntityComponent.Effect),
                (effectEntity, _, __, ___) => effectEntity.RemoveComponent((int)EntityComponent.Effect),
                referencedKeepAlive: false, referencingKeepAlive: true);

            AppliedEffectsToSourceAbilityRelationship = new EntityRelationship<GameEntity>(
                nameof(AppliedEffectsToSourceAbilityRelationship),
                Effects,
                Abilities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((EffectComponent)component).SourceAbilityId,
                    (int)EntityComponent.Effect),
                (effectEntity, _, __, ___) => effectEntity.Effect.SourceAbilityId = null);

            EffectApplicationSystem = new EffectApplicationSystem(new PropertyValueCalculator());
            queue.Add<AbilityActivatedMessage>(EffectApplicationSystem,
                AbilityActivationSystem.AbilityActivatedMessageName, 0);
            queue.Add<EntityAddedMessage<GameEntity>>(EffectApplicationSystem,
                AffectableEntities.GetEntityAddedMessageName(), 0);
            queue.Add<EntityAddedMessage<GameEntity>>(EffectApplicationSystem,
                AppliedEffectsToAffectableEntityRelationship.GetEntityAddedMessageName(), 0);
            queue.Add<EntityRemovedMessage<GameEntity>>(EffectApplicationSystem,
                AppliedEffectsToAffectableEntityRelationship.GetEntityRemovedMessageName(), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int?>>(EffectApplicationSystem,
                AppliedEffectsToAffectableEntityRelationship.GetPropertyValueChangedMessageName(
                    nameof(EffectComponent.Amount)), 0);
        }
    }
}
