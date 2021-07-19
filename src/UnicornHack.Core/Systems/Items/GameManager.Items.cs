using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> LevelItems { get; private set; }
        public EntityGroup<GameEntity> ContainedItems { get; private set; }
        public UniqueEntityIndex<GameEntity, (int, byte, byte)> LevelItemsToLevelCellIndex { get; private set; }
        public EntityRelationship<GameEntity> EntityItemsToContainerRelationship { get; private set; }
        public EntityRelationship<GameEntity> LevelItemsToLevelRelationship { get; private set; }
        public ItemMovingSystem ItemMovingSystem { get; private set; }
        public ItemUsageSystem ItemUsageSystem { get; private set; }

        private void InitializeItems(SequentialMessageQueue<GameManager> queue)
        {
            Add<ItemComponent>(EntityComponent.Item, poolSize: 32);

            ContainedItems = CreateGroup(nameof(ContainedItems),
                new EntityMatcher<GameEntity>()
                    .AllOf((int)EntityComponent.Item, (int)EntityComponent.Physical)
                    .NoneOf((int)EntityComponent.Position));

            LevelItems = CreateGroup(nameof(LevelItems),
                new EntityMatcher<GameEntity>()
                    .AllOf((int)EntityComponent.Item, (int)EntityComponent.Physical, (int)EntityComponent.Position));

            LevelItemsToLevelCellIndex = new UniqueEntityIndex<GameEntity, (int, byte, byte)>(
                nameof(LevelItemsToLevelCellIndex),
                LevelItems,
                new KeyValueGetter<GameEntity, (int, byte, byte)>(
                    (entity, changes, getOldValue, matcher) =>
                    {
                        if (!matcher.TryGetValue<int>(
                                entity, (int)EntityComponent.Position, nameof(PositionComponent.LevelId), changes,
                                getOldValue, out var levelId)
                            || !matcher.TryGetValue<byte>(
                                entity, (int)EntityComponent.Position, nameof(PositionComponent.LevelX), changes,
                                getOldValue, out var levelX)
                            || !matcher.TryGetValue<byte>(
                                entity, (int)EntityComponent.Position, nameof(PositionComponent.LevelY), changes,
                                getOldValue, out var levelY))
                        {
                            return ((0, 0, 0), false);
                        }

                        return ((levelId, levelX, levelY), true);
                    },
                    new PropertyMatcher()
                        .With(component => ((PositionComponent)component).LevelId, (int)EntityComponent.Position)
                        .With(component => ((PositionComponent)component).LevelX, (int)EntityComponent.Position)
                        .With(component => ((PositionComponent)component).LevelY, (int)EntityComponent.Position)
                ));

            LevelItemsToLevelRelationship = new EntityRelationship<GameEntity>(
                nameof(LevelItemsToLevelRelationship),
                LevelItems,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((PositionComponent)component).LevelId,
                    (int)EntityComponent.Position),
                (effectEntity, _, _) => effectEntity.RemoveComponent(EntityComponent.Position));

            EntityItemsToContainerRelationship = new EntityRelationship<GameEntity>(
                nameof(EntityItemsToContainerRelationship),
                ContainedItems,
                AffectableEntities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((ItemComponent)component).ContainerId,
                    (int)EntityComponent.Item),
                (effectEntity, _, _) => effectEntity.RemoveComponent((int)EntityComponent.Item),
                referencedKeepAlive: false,
                referencingKeepAlive: true);

            ItemMovingSystem = new ItemMovingSystem();
            queue.Add<MoveItemMessage>(ItemMovingSystem, MoveItemMessage.Name, 0);
            queue.Add<TraveledMessage>(ItemMovingSystem, TraveledMessage.Name, 0);
            queue.Add<DiedMessage>(ItemMovingSystem, DiedMessage.Name, 0);

            ItemUsageSystem = new ItemUsageSystem();
            queue.Add(ItemUsageSystem, EquipItemMessage.Name, 0);

            queue.Add<EntityAddedMessage<GameEntity>>(
                AbilityActivationSystem, EntityItemsToContainerRelationship.GetEntityAddedMessageName(), 0);
            queue.Add<EntityRemovedMessage<GameEntity>>(
                AbilityActivationSystem, EntityItemsToContainerRelationship.GetEntityRemovedMessageName(), 0);
        }
    }
}
