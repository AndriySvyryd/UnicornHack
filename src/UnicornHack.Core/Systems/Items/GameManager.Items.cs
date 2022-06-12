using System.Collections.Generic;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> LevelItems { get; private set; }
        public EntityGroup<GameEntity> ContainedItems { get; private set; }
        public LookupEntityRelationship<GameEntity, Point, Dictionary<Point, GameEntity>> LevelItemsToLevelCellRelationship { get; private set; }
        public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> EntityItemsToContainerRelationship { get; private set; }
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

            LevelItemsToLevelCellRelationship = new(
                nameof(LevelItemsToLevelCellRelationship),
                LevelItems,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((PositionComponent)component).LevelId,
                    (int)EntityComponent.Position),
                new KeyValueGetter<GameEntity, Point>(
                    (change, matcher, valueType) =>
                    {
                        if (!matcher.TryGetValue<byte>(
                                change, (int)EntityComponent.Position, nameof(PositionComponent.LevelX), valueType,
                                out var levelX)
                            || !matcher.TryGetValue<byte>(
                                change, (int)EntityComponent.Position, nameof(PositionComponent.LevelY), valueType,
                                out var levelY))
                        {
                            return (new Point(0, 0), false);
                        }

                        return (new Point(levelX, levelY), true);
                    },
                    new PropertyMatcher<GameEntity>()
                        .With(component => ((PositionComponent)component).LevelX, (int)EntityComponent.Position)
                        .With(component => ((PositionComponent)component).LevelY, (int)EntityComponent.Position)
                ),
                (itemEntity, _) => itemEntity.RemoveComponent(EntityComponent.Position),
                levelEntity => (Dictionary<Point, GameEntity>)levelEntity.Level.Items);

            EntityItemsToContainerRelationship = new(
                nameof(EntityItemsToContainerRelationship),
                ContainedItems,
                AffectableEntities,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((ItemComponent)component).ContainerId,
                    (int)EntityComponent.Item),
                (effectEntity, _) => effectEntity.RemoveComponent((int)EntityComponent.Item),
                containerEntity => (HashSet<GameEntity>)(containerEntity.Being.Items
                                                         ?? containerEntity.Item.Items
                                                         ?? containerEntity.Physical.Items
                                                         ?? containerEntity.Sensor.Items),
                keepPrincipalAlive: false,
                keepDependentAlive: true);

            ItemMovingSystem = new ItemMovingSystem();
            queue.Register<MoveItemMessage>(ItemMovingSystem, MoveItemMessage.Name, 0);
            queue.Register<TraveledMessage>(ItemMovingSystem, TraveledMessage.Name, 0);
            queue.Register<DiedMessage>(ItemMovingSystem, DiedMessage.Name, 0);

            ItemUsageSystem = new ItemUsageSystem();
            queue.Register(ItemUsageSystem, EquipItemMessage.Name, 0);

            queue.Register<EntityAddedMessage<GameEntity>>(
                AbilityActivationSystem, EntityItemsToContainerRelationship.Dependents.GetEntityAddedMessageName(), 0);
            queue.Register<EntityRemovedMessage<GameEntity>>(
                AbilityActivationSystem, EntityItemsToContainerRelationship.Dependents.GetEntityRemovedMessageName(),
                0);
        }
    }
}
