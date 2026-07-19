using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> Rooms
    {
        get;
        private set;
    } = null!;

    public EntityGroup<GameEntity> Connections
    {
        get;
        private set;
    } = null!;

    public EntityGroup<GameEntity> RoomEntities
    {
        get;
        private set;
    } = null!;

    public LookupEntityRelationship<GameEntity, Point, SortedDictionary<Point, GameEntity>>
        ConnectionsToCellRelationship
    {
        get;
        private set;
    } = null!;

    public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> IncomingConnectionsToRoomRelationship
    {
        get;
        private set;
    } = null!;

    public TravelSystem TravelSystem
    {
        get;
        private set;
    } = null!;

    private void InitializeLevels(SequentialMessageQueue<GameManager> queue)
    {
        Add<RoomComponent>(EntityComponent.Room, poolSize: 32, maxPoolSize: 64);
        Add<ConnectionComponent>(EntityComponent.Connection, poolSize: 64, maxPoolSize: 128);
        Add<PositionComponent>(EntityComponent.Position, poolSize: 512, maxPoolSize: 1024);

        Rooms = CreateGroup(nameof(Rooms),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Room), capacity: 32);
        Connections = CreateGroup(nameof(Connections),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position, (int)EntityComponent.Connection), capacity: 64);
        RoomEntities = CreateGroup(nameof(RoomEntities),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position), capacity: 512);

        ConnectionsToCellRelationship = new(
            nameof(ConnectionsToCellRelationship),
            Connections,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((PositionComponent)component).RoomId,
                (int)EntityComponent.Position),
            CreateCellKeyValueGetter(),
            (connectionEntity, _) => connectionEntity.RemoveComponent(EntityComponent.Position),
            levelEntity => (SortedDictionary<Point, GameEntity>)levelEntity.Room!.Connections,
            keepDependentAlive: true);

        IncomingConnectionsToRoomRelationship = new(
            nameof(IncomingConnectionsToRoomRelationship),
            Connections,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((ConnectionComponent)component).TargetRoomId,
                (int)EntityComponent.Connection),
            (effectEntity, _) => effectEntity.RemoveComponent(EntityComponent.Position),
            levelEntity => (HashSet<GameEntity>)levelEntity.Room!.IncomingConnections);

        TravelSystem = new TravelSystem();
        queue.Register<TravelMessage>(TravelSystem, TravelMessage.Name, 0);
        queue.Register<DiedMessage>(TravelSystem, DiedMessage.Name, 6);
    }

    // The by-cell index key for a position: the (CellX, CellY) point, or NO key when the entity is in a room
    // but has no cell of its own, such as an item carried inside a container. Returning
    // hasKey=false keeps such an entity's PositionComponent while excluding it from the room's by-cell
    // actor/item/connection indexes.
    private static KeyValueGetter<GameEntity, Point> CreateCellKeyValueGetter()
        => new(
            static (change, matcher, valueType) =>
            {
                if (!matcher.TryGetValue<byte?>(
                        change, (int)EntityComponent.Position, nameof(PositionComponent.CellX), valueType,
                        out var cellX)
                    || !matcher.TryGetValue<byte?>(
                        change, (int)EntityComponent.Position, nameof(PositionComponent.CellY), valueType,
                        out var cellY)
                    || cellX is not { } x
                    || cellY is not { } y)
                {
                    return (new Point(0, 0), false);
                }

                return (new Point(x, y), true);
            },
            new PropertyMatcher<GameEntity>()
                .With(component => ((PositionComponent)component).CellX, (int)EntityComponent.Position)
                .With(component => ((PositionComponent)component).CellY, (int)EntityComponent.Position));
}
