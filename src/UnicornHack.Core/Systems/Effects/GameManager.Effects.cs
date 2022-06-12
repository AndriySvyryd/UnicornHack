using System.Collections.Generic;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> Effects { get; private set; }
        public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> AppliedEffectsToAffectableEntityRelationship { get; private set; }
        public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> AppliedEffectsToSourceAbilityRelationship { get; private set; }
        public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> EffectsToContainingAbilityRelationship { get; private set; }
        public EffectApplicationSystem EffectApplicationSystem { get; private set; }

        private void InitializeEffects(SequentialMessageQueue<GameManager> queue)
        {
            Add<EffectComponent>(EntityComponent.Effect, poolSize: 32);

            Effects = CreateGroup(nameof(Effects),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Effect));

            AppliedEffectsToAffectableEntityRelationship = new(
                nameof(AppliedEffectsToAffectableEntityRelationship),
                Effects,
                AffectableEntities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((EffectComponent)component).AffectedEntityId,
                    (int)EntityComponent.Effect),
                (effectEntity, _) => effectEntity.RemoveComponent((int)EntityComponent.Effect),
                containerEntity => (HashSet<GameEntity>)(containerEntity.Being.AppliedEffects
                     ?? containerEntity.Item.AppliedEffects
                     ?? containerEntity.Physical.AppliedEffects
                     ?? containerEntity.Sensor.AppliedEffects),
                keepPrincipalAlive: false, keepDependentAlive: true);

            EffectsToContainingAbilityRelationship = new(
                nameof(EffectsToContainingAbilityRelationship),
                Effects,
                Abilities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((EffectComponent)component).ContainingAbilityId,
                    (int)EntityComponent.Effect),
                (effectEntity, _) => effectEntity.RemoveComponent((int)EntityComponent.Effect),
                abilityEntity => (HashSet<GameEntity>)abilityEntity.Ability.Effects,
                effectEntity => effectEntity.Effect.ContainingAbility,
                keepPrincipalAlive: false, keepDependentAlive: true);

            AppliedEffectsToSourceAbilityRelationship = new(
                nameof(AppliedEffectsToSourceAbilityRelationship),
                Effects,
                Abilities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((EffectComponent)component).SourceAbilityId,
                    (int)EntityComponent.Effect),
                (effectEntity, change) =>
                {
                    var sourceAbility = change.RemovedComponent as AbilityComponent
                                        ?? effectEntity.Manager.FindEntity(effectEntity.Effect.SourceAbilityId)
                                            ?.Ability;
                    if (sourceAbility?.IsActive == true)
                    {
                        effectEntity.Effect = null;
                    }
                    else
                    {
                        effectEntity.Effect.SourceAbilityId = null;
                    }
                });

            EffectApplicationSystem = new EffectApplicationSystem();
            queue.Register<AbilityActivatedMessage>(EffectApplicationSystem,
                AbilityActivatedMessage.Name, 0);
            queue.Register<ApplyEffectMessage>(EffectApplicationSystem,
                ApplyEffectMessage.Name, 0);
            queue.Register<EntityAddedMessage<GameEntity>>(EffectApplicationSystem,
                AffectableEntities.GetEntityAddedMessageName(), 0);
            // TODO: Only listen for EffectsToContainingAbilityRelationship.GetEntityAddedMessageName()
            queue.Register<EntityAddedMessage<GameEntity>>(EffectApplicationSystem,
                Effects.GetEntityAddedMessageName(), 0);
            queue.Register<EntityRemovedMessage<GameEntity>>(EffectApplicationSystem,
                Effects.GetEntityRemovedMessageName(), 0);
            queue.Register<PropertyValueChangedMessage<GameEntity, int?>>(EffectApplicationSystem,
                Effects.GetPropertyValueChangedMessageName(nameof(EffectComponent.AppliedAmount)), 0);
            queue.Register<PropertyValueChangedMessage<GameEntity, string>>(EffectApplicationSystem,
                Effects.GetPropertyValueChangedMessageName(nameof(EffectComponent.Amount)), 0);

            queue.Register<EntityAddedMessage<GameEntity>>(AbilityActivationSystem,
                Effects.GetEntityAddedMessageName(), 1);
            queue.Register<EntityRemovedMessage<GameEntity>>(AbilityActivationSystem,
                Effects.GetEntityRemovedMessageName(), 1);
        }
    }
}
