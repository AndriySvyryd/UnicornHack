using System.Collections.Generic;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> Abilities { get; private set; }
        public EntityGroup<GameEntity> AffectableEntities { get; private set; }
        public UniqueEntityIndex<GameEntity, (int, string)> AffectableAbilitiesIndex { get; private set; }
        public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> AbilitiesToAffectableRelationship { get; private set; }
        public AbilityActivationSystem AbilityActivationSystem { get; private set; }
        public AbilitySlottingSystem AbilitySlottingSystem { get; private set; }

        private void InitializeAbilities(SequentialMessageQueue<GameManager> queue)
        {
            Add<AbilityComponent>(EntityComponent.Ability, poolSize: 32);

            AffectableEntities = CreateGroup(nameof(AffectableEntities),
                new EntityMatcher<GameEntity>().AnyOf(
                    (int)EntityComponent.Being,
                    (int)EntityComponent.Item,
                    (int)EntityComponent.Physical,
                    (int)EntityComponent.Sensor));

            Abilities = CreateGroup(nameof(Abilities),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Ability));

            AffectableAbilitiesIndex = new (
                nameof(AffectableAbilitiesIndex),
                Abilities,
                new KeyValueGetter<GameEntity, (int, string)>(
                    (change, matcher, valueType) =>
                    {
                        if (!matcher.TryGetValue<int?>(
                                change, (int)EntityComponent.Ability, nameof(AbilityComponent.OwnerId),
                                valueType, out var ownerId)
                            || !ownerId.HasValue
                            || !matcher.TryGetValue<string>(
                                change, (int)EntityComponent.Ability, nameof(AbilityComponent.Name),
                                valueType, out var name)
                            || name == null)
                        {
                            return ((0, null), false);
                        }

                        return ((ownerId.Value, name), true);
                    },
                    new PropertyMatcher<GameEntity>()
                        .With(component => ((AbilityComponent)component).OwnerId, (int)EntityComponent.Ability)
                        .With(component => ((AbilityComponent)component).Name, (int)EntityComponent.Ability)
                ));

            AbilitiesToAffectableRelationship = new(
                nameof(AbilitiesToAffectableRelationship),
                Abilities,
                AffectableEntities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((AbilityComponent)component).OwnerId,
                    (int)EntityComponent.Ability),
                (effectEntity, _) => effectEntity.RemoveComponent((int)EntityComponent.Ability),
                containerEntity => (HashSet<GameEntity>)(containerEntity.Being.Abilities
                     ?? containerEntity.Item.Abilities
                     ?? containerEntity.Physical.Abilities
                     ?? containerEntity.Sensor.Abilities),
                keepPrincipalAlive: false,
                keepDependentAlive: true);

            AbilityActivationSystem = new AbilityActivationSystem();
            queue.Register<ActivateAbilityMessage>(AbilityActivationSystem,
                ActivateAbilityMessage.Name, 0);
            queue.Register<DeactivateAbilityMessage>(AbilityActivationSystem,
                DeactivateAbilityMessage.Name, 0);
            queue.Register<ItemEquippedMessage>(AbilityActivationSystem, ItemEquippedMessage.Name, 0);
            queue.Register<DiedMessage>(AbilityActivationSystem, DiedMessage.Name, 2);
            queue.Register<LeveledUpMessage>(AbilityActivationSystem, LeveledUpMessage.Name, 0);
            queue.Register<EntityAddedMessage<GameEntity>>(
                AbilityActivationSystem, AbilitiesToAffectableRelationship.Dependents.GetEntityAddedMessageName(), 0);
            queue.Register<EntityRemovedMessage<GameEntity>>(
                AbilityActivationSystem, AbilitiesToAffectableRelationship.Dependents.GetEntityRemovedMessageName(), 0);

            AbilitySlottingSystem = new AbilitySlottingSystem();
            queue.Register<SetAbilitySlotMessage>(AbilitySlottingSystem,
                SetAbilitySlotMessage.Name, 0);
            queue.Register<PropertyValueChangedMessage<GameEntity, bool>>(AbilitySlottingSystem,
                AbilitiesToAffectableRelationship.Dependents.GetPropertyValueChangedMessageName(nameof(AbilityComponent.IsUsable)),
                0);
            queue.Register<PropertyValueChangedMessage<GameEntity, int>>(AbilitySlottingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(PhysicalComponent.Capacity)),
                0);
            queue.Register<EntityAddedMessage<GameEntity>>(
                AbilitySlottingSystem, AbilitiesToAffectableRelationship.Dependents.GetEntityAddedMessageName(), 2);
        }
    }
}
