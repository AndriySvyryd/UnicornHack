using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> RoomItems
    {
        get;
        private set;
    } = null!;

    public EntityGroup<GameEntity> ContainedItems
    {
        get;
        private set;
    } = null!;

    public LookupEntityRelationship<GameEntity, Point, SortedDictionary<Point, GameEntity>>
        RoomItemsToRoomCellRelationship
    {
        get;
        private set;
    } = null!;

    public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> EntityItemsToContainerRelationship
    {
        get;
        private set;
    } = null!;

    public ItemMovingSystem ItemMovingSystem
    {
        get;
        private set;
    } = null!;

    public ItemUsageSystem ItemUsageSystem
    {
        get;
        private set;
    } = null!;

    private void InitializeItems(SequentialMessageQueue<GameManager> queue)
    {
        Add<ItemComponent>(EntityComponent.Item, poolSize: 32);

        ContainedItems = CreateGroup(nameof(ContainedItems),
            new EntityMatcher<GameEntity>()
                .AllOf((int)EntityComponent.Item, (int)EntityComponent.Physical)
                .NoneOf((int)EntityComponent.Position));

        RoomItems = CreateGroup(nameof(RoomItems),
            new EntityMatcher<GameEntity>()
                .AllOf((int)EntityComponent.Item, (int)EntityComponent.Physical, (int)EntityComponent.Position));

        RoomItemsToRoomCellRelationship = new(
            nameof(RoomItemsToRoomCellRelationship),
            RoomItems,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((PositionComponent)component).RoomId,
                (int)EntityComponent.Position),
            new KeyValueGetter<GameEntity, Point>(
                (change, matcher, valueType) =>
                {
                    if (!matcher.TryGetValue<byte>(
                            change, (int)EntityComponent.Position, nameof(PositionComponent.CellX), valueType,
                            out var levelX)
                        || !matcher.TryGetValue<byte>(
                            change, (int)EntityComponent.Position, nameof(PositionComponent.CellY), valueType,
                            out var levelY))
                    {
                        return (new Point(0, 0), false);
                    }

                    return (new Point(levelX, levelY), true);
                },
                new PropertyMatcher<GameEntity>()
                    .With(component => ((PositionComponent)component).CellX, (int)EntityComponent.Position)
                    .With(component => ((PositionComponent)component).CellY, (int)EntityComponent.Position)
            ),
            (itemEntity, _) => itemEntity.RemoveComponent(EntityComponent.Position),
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.Items);

        EntityItemsToContainerRelationship = new(
            nameof(EntityItemsToContainerRelationship),
            ContainedItems,
            AffectableEntities,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((ItemComponent)component).ContainerId,
                (int)EntityComponent.Item),
            (effectEntity, _) => effectEntity.RemoveComponent((int)EntityComponent.Item),
            containerEntity => (HashSet<GameEntity>)(containerEntity.Being!.Items
                                                     ?? containerEntity.Item!.Items
                                                     ?? containerEntity.Physical!.Items
                                                     ?? containerEntity.Sensor!.Items),
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
