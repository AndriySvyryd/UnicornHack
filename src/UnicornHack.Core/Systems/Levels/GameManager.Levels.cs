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
        Add<RoomComponent>(EntityComponent.Room, poolSize: 4);
        Add<ConnectionComponent>(EntityComponent.Connection, poolSize: 8);
        Add<PositionComponent>(EntityComponent.Position, poolSize: 32);

        Rooms = CreateGroup(nameof(Rooms),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Room));
        Connections = CreateGroup(nameof(Connections),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position, (int)EntityComponent.Connection));
        RoomEntities = CreateGroup(nameof(RoomEntities),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position));

        ConnectionsToCellRelationship = new(
            nameof(ConnectionsToCellRelationship),
            Connections,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((PositionComponent)component).RoomId,
                (int)EntityComponent.Position),
            new KeyValueGetter<GameEntity, Point>(
                (change, matcher, valueType) =>
                {
                    if (!matcher.TryGetValue<byte>(
                            change, (int)EntityComponent.Position, nameof(PositionComponent.CellX), valueType,
                            out var cellX)
                        || !matcher.TryGetValue<byte>(
                            change, (int)EntityComponent.Position, nameof(PositionComponent.CellY), valueType,
                            out var cellY))
                    {
                        return (new Point(0, 0), false);
                    }

                    return (new Point(cellX, cellY), true);
                },
                new PropertyMatcher<GameEntity>()
                    .With(component => ((PositionComponent)component).CellX, (int)EntityComponent.Position)
                    .With(component => ((PositionComponent)component).CellY, (int)EntityComponent.Position)
            ),
            (connectionEntity, _) => connectionEntity.RemoveComponent(EntityComponent.Position),
            levelEntity => (SortedDictionary<Point, GameEntity>)levelEntity.Room!.Connections);

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
}
