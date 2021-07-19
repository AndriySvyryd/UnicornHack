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
        public SortedLookupEntityIndex<GameEntity, int, int> SlottedAbilitiesIndex { get; private set; }
        public UniqueEntityIndex<GameEntity, (int, string)> AffectableAbilitiesIndex { get; private set; }
        public EntityRelationship<GameEntity> AbilitiesToAffectableRelationship { get; private set; }
        public AbilityActivationSystem AbilityActivationSystem { get; private set; }
        public AbilitySlottingSystem AbilitySlottingSystem { get; private set; }

        private void InitializeAbilities(SequentialMessageQueue<GameManager> queue)
        {
            Add<AbilityComponent>(EntityComponent.Ability, poolSize: 32);

            AffectableEntities = CreateGroup(nameof(AffectableEntities),
                new EntityMatcher<GameEntity>().AnyOf(
                    (int)EntityComponent.Being, (int)EntityComponent.Item, (int)EntityComponent.Physical,
                    (int)EntityComponent.Sensor));

            Abilities = CreateGroup(nameof(Abilities),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Ability));

            SlottedAbilitiesIndex = new SortedLookupEntityIndex<GameEntity, int, int>(
                nameof(SlottedAbilitiesIndex),
                Abilities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((AbilityComponent)component).OwnerId,
                    (int)EntityComponent.Ability),
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((AbilityComponent)component).Slot,
                    (int)EntityComponent.Ability));

            AffectableAbilitiesIndex = new UniqueEntityIndex<GameEntity, (int, string)>(
                nameof(AffectableAbilitiesIndex),
                Abilities,
                new KeyValueGetter<GameEntity, (int, string)>(
                    (entity, changes, getOldValue, matcher) =>
                    {
                        if (!matcher.TryGetValue<int?>(
                                entity, (int)EntityComponent.Ability, nameof(AbilityComponent.OwnerId), changes,
                                getOldValue, out var ownerId)
                            || !ownerId.HasValue
                            || !matcher.TryGetValue<string>(
                                entity, (int)EntityComponent.Ability, nameof(AbilityComponent.Name), changes,
                                getOldValue, out var name)
                            || name == null)
                        {
                            return ((0, null), false);
                        }

                        return ((ownerId.Value, name), true);
                    },
                    new PropertyMatcher()
                        .With(component => ((AbilityComponent)component).OwnerId, (int)EntityComponent.Ability)
                        .With(component => ((AbilityComponent)component).Name, (int)EntityComponent.Ability)
                ));

            AbilitiesToAffectableRelationship = new EntityRelationship<GameEntity>(
                nameof(AbilitiesToAffectableRelationship),
                Abilities,
                AffectableEntities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((AbilityComponent)component).OwnerId,
                    (int)EntityComponent.Ability),
                (effectEntity, _, __) => effectEntity.RemoveComponent((int)EntityComponent.Ability),
                referencedKeepAlive: false,
                referencingKeepAlive: true);

            AbilityActivationSystem = new AbilityActivationSystem();
            queue.Add<ActivateAbilityMessage>(AbilityActivationSystem,
                ActivateAbilityMessage.Name, 0);
            queue.Add<DeactivateAbilityMessage>(AbilityActivationSystem,
                DeactivateAbilityMessage.Name, 0);
            queue.Add<ItemEquippedMessage>(AbilityActivationSystem, ItemEquippedMessage.Name, 0);
            queue.Add<DiedMessage>(AbilityActivationSystem, DiedMessage.Name, 2);
            queue.Add<LeveledUpMessage>(AbilityActivationSystem, LeveledUpMessage.Name, 0);
            queue.Add<EntityAddedMessage<GameEntity>>(
                AbilityActivationSystem, AbilitiesToAffectableRelationship.GetEntityAddedMessageName(), 0);
            queue.Add<EntityRemovedMessage<GameEntity>>(
                AbilityActivationSystem, AbilitiesToAffectableRelationship.GetEntityRemovedMessageName(), 0);

            AbilitySlottingSystem = new AbilitySlottingSystem();
            queue.Add<SetAbilitySlotMessage>(AbilitySlottingSystem,
                SetAbilitySlotMessage.Name, 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, bool>>(AbilitySlottingSystem,
                AbilitiesToAffectableRelationship.GetPropertyValueChangedMessageName(nameof(AbilityComponent.IsUsable)),
                0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(AbilitySlottingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(PhysicalComponent.Capacity)),
                0);
            queue.Add<EntityAddedMessage<GameEntity>>(
                AbilitySlottingSystem, AbilitiesToAffectableRelationship.GetEntityAddedMessageName(), 2);
        }
    }
}
