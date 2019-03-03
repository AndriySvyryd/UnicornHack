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
        public UniqueEntityIndex<GameEntity, (int, int)> SlottedAbilitiesIndex { get; private set; }
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

            SlottedAbilitiesIndex = new UniqueEntityIndex<GameEntity, (int, int)>(
                nameof(SlottedAbilitiesIndex),
                Abilities,
                new KeyValueGetter<GameEntity, (int, int)>(
                    (entity, changes, getOldValue, matcher) =>
                    {
                        if (!matcher.TryGetValue<int?>(
                                entity, (int)EntityComponent.Ability, nameof(AbilityComponent.OwnerId), changes,
                                getOldValue, out var ownerId)
                            || !ownerId.HasValue
                            || !matcher.TryGetValue<int?>(
                                entity, (int)EntityComponent.Ability, nameof(AbilityComponent.Slot), changes,
                                getOldValue, out var slot)
                            || !slot.HasValue)
                        {
                            return ((0, 0), false);
                        }

                        return ((ownerId.Value, slot.Value), true);
                    },
                    new PropertyMatcher()
                        .With(component => ((AbilityComponent)component).OwnerId, (int)EntityComponent.Ability)
                        .With(component => ((AbilityComponent)component).Slot, (int)EntityComponent.Ability)
                ));

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
                AbilityActivationSystem.ActivateAbilityMessageName, 0);
            queue.Add<DeactivateAbilityMessage>(AbilityActivationSystem,
                AbilityActivationSystem.DeactivateAbilityMessageName, 0);
            queue.Add<ItemEquippedMessage>(AbilityActivationSystem, ItemUsageSystem.ItemEquippedMessageName, 3);
            queue.Add<DiedMessage>(AbilityActivationSystem, LivingSystem.DiedMessageName, 2);
            queue.Add<LeveledUpMessage>(AbilityActivationSystem, XPSystem.LeveledUpMessageName, 0);
            queue.Add<EntityAddedMessage<GameEntity>>(
                AbilityActivationSystem, AbilitiesToAffectableRelationship.GetEntityAddedMessageName(), 0);
            queue.Add<EntityRemovedMessage<GameEntity>>(
                AbilityActivationSystem, AbilitiesToAffectableRelationship.GetEntityRemovedMessageName(), 0);

            AbilitySlottingSystem = new AbilitySlottingSystem();
            queue.Add<SetAbilitySlotMessage>(AbilitySlottingSystem,
                AbilitySlottingSystem.SetAbilitySlotMessageName, 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, bool>>(AbilitySlottingSystem,
                AbilitiesToAffectableRelationship.GetPropertyValueChangedMessageName(nameof(AbilityComponent.IsUsable)),
                0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(AbilitySlottingSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.AbilitySlotCount)),
                0);
        }
    }
}
