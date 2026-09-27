using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> Players
    {
        get;
        private set;
    } = null!;

    public EntityGroup<GameEntity> RoomActors
    {
        get;
        private set;
    } = null!;

    public LookupEntityRelationship<GameEntity, Point, SortedDictionary<Point, GameEntity>>
        RoomActorsToRoomCellRelationship
    {
        get;
        private set;
    } = null!;

    public SkipLookupEntityRelationship<GameEntity, int, SortedDictionary<int, GameEntity>>
        SlottedAbilitiesToRoomActorsRelationship
    {
        get;
        private set;
    } = null!;

    public PlayerSystem PlayerSystem
    {
        get;
        private set;
    } = null!;

    public PlayerChoiceSystem PlayerChoiceSystem
    {
        get;
        private set;
    } = null!;

    public AISystem AISystem
    {
        get;
        private set;
    } = null!;

    private void InitializeActors(SequentialMessageQueue<GameEntity> queue)
    {
        Add<PlayerComponent>(EntityComponent.Player, poolSize: 1);
        Add<AIComponent>(EntityComponent.AI, poolSize: 256, maxPoolSize: 512);

        Players = CreateGroup(nameof(Players),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Player), capacity: 1);

        RoomActors = CreateGroup(nameof(RoomActors),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position, (int)EntityComponent.Being)
                .AnyOf((int)EntityComponent.AI, (int)EntityComponent.Player), capacity: 256);

        RoomActorsToRoomCellRelationship = new(
            nameof(RoomActorsToRoomCellRelationship),
            RoomActors,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((PositionComponent)component).RoomId,
                (int)EntityComponent.Position),
            CreateCellKeyValueGetter(),
            (actorEntity, _) => actorEntity.RemoveComponent(EntityComponent.Position),
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.Actors,
            keepDependentAlive: true);

        AISystem = new AISystem();
        queue.Register<PerformActionMessage>(AISystem, PerformActionMessage.AIName, 0);
        queue.Register<DecideNextActionMessage>(AISystem, DecideNextActionMessage.Name, 0);
        queue.Register<DelayMessage>(AISystem, DelayMessage.Name, 0);
        queue.Register<ItemEquippedMessage>(AISystem, ItemEquippedMessage.Name, 5);
        queue.Register<DiedMessage>(AISystem, DiedMessage.Name, 4);
        queue.Register<EntityAddedMessage<GameEntity>>(AISystem,
            AbilitiesToAffectableRelationship.Dependents.GetEntityAddedMessageName(),
            1);
        queue.Register<PropertyValueChangedMessage<GameEntity, bool>>(AISystem,
            AbilitiesToAffectableRelationship.Dependents.GetPropertyValueChangedMessageName(
                nameof(AbilityComponent.IsUsable)),
            1);

        PlayerChoiceSystem = new PlayerChoiceSystem();
        PlayerSystem = new PlayerSystem();
        queue.Register<PerformActionMessage>(PlayerSystem, PerformActionMessage.PlayerName, 0);
        queue.Register<DelayMessage>(PlayerSystem, DelayMessage.Name, 1);
        queue.Register<DiedMessage>(PlayerSystem, DiedMessage.Name, 5);
        queue.Register<TraveledMessage>(PlayerSystem, TraveledMessage.Name, 1);
    }
}
