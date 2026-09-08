using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> KnownPositions
    {
        get;
        private set;
    } = null!;

    public LookupEntityRelationship<GameEntity, Point, SortedDictionary<Point, GameEntity>>
        KnownActorsToCellRelationship
    {
        get;
        private set;
    } = null!;

    public LookupEntityRelationship<GameEntity, Point, SortedDictionary<Point, GameEntity>>
        KnownItemsToCellRelationship
    {
        get;
        private set;
    } = null!;

    public LookupEntityRelationship<GameEntity, Point, SortedDictionary<Point, GameEntity>>
        KnownConnectionsToCellRelationship
    {
        get;
        private set;
    } = null!;

    public UniqueEntityRelationship<GameEntity> RoomKnowledgeToRoomRelationship
    {
        get;
        private set;
    } = null!;

    public UniqueEntityIndex<GameEntity, int> XPCooldownEntities
    {
        get;
        private set;
    } = null!;

    public KnowledgeSystem KnowledgeSystem
    {
        get;
        private set;
    } = null!;

    public XPSystem XPSystem
    {
        get;
        private set;
    } = null!;

    public LoggingSystem LoggingSystem
    {
        get;
        private set;
    } = null!;

    private void InitializeKnowledge(SequentialMessageQueue<GameEntity> queue)
    {
        Add<KnowledgeComponent>(EntityComponent.Knowledge, poolSize: 32, maxPoolSize: 64);

        KnownPositions = CreateGroup(nameof(KnownPositions),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Knowledge, (int)EntityComponent.Position), capacity: 32);

        KnownActorsToCellRelationship = new(
            nameof(KnownActorsToCellRelationship),
            KnownPositions,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((PositionComponent)component).RoomId,
                (int)EntityComponent.Position),
            CreateCellKeyValueGetter(),
            (knowledgeEntity, _) =>
            {
                knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                knowledgeEntity.RemoveComponent(EntityComponent.Position);
            },
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.KnownActors,
            secondaryPrincipalGroup: RoomActors,
            secondaryPrincipalKeyValueGetter: new SimpleKeyValueGetter<GameEntity, int>(
                component => ((KnowledgeComponent)component).KnownEntityId,
                (int)EntityComponent.Knowledge),
            keepDependentAlive: true);

        KnownItemsToCellRelationship = new(
            nameof(KnownItemsToCellRelationship),
            KnownPositions,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((PositionComponent)component).RoomId,
                (int)EntityComponent.Position),
            CreateCellKeyValueGetter(),
            (knowledgeEntity, _) =>
            {
                knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                knowledgeEntity.RemoveComponent(EntityComponent.Position);
            },
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.KnownItems,
            secondaryPrincipalGroup: RoomItems,
            secondaryPrincipalKeyValueGetter: new SimpleKeyValueGetter<GameEntity, int>(
                component => ((KnowledgeComponent)component).KnownEntityId,
                (int)EntityComponent.Knowledge),
            keepDependentAlive: true);

        KnownConnectionsToCellRelationship = new(
            nameof(KnownConnectionsToCellRelationship),
            KnownPositions,
            Rooms,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((PositionComponent)component).RoomId,
                (int)EntityComponent.Position),
            CreateCellKeyValueGetter(),
            (knowledgeEntity, _) =>
            {
                knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                knowledgeEntity.RemoveComponent(EntityComponent.Position);
            },
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.KnownConnections,
            secondaryPrincipalGroup: Connections,
            secondaryPrincipalKeyValueGetter: new SimpleKeyValueGetter<GameEntity, int>(
                component => ((KnowledgeComponent)component).KnownEntityId,
                (int)EntityComponent.Knowledge),
            keepDependentAlive: true);

        RoomKnowledgeToRoomRelationship = new(
            nameof(RoomKnowledgeToRoomRelationship),
            KnownPositions,
            RoomEntities,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((KnowledgeComponent)component).KnownEntityId,
                (int)EntityComponent.Knowledge),
            (knowledgeEntity, _) =>
            {
                knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                knowledgeEntity.RemoveComponent(EntityComponent.Position);
            },
            levelEntity => levelEntity.Position!.Knowledge);

        XPCooldownEntities = new(
            nameof(XPCooldownEntities),
            TemporalEntities,
            new KeyValueGetter<GameEntity, int>(
                (change, matcher, valueType) =>
                {
                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.Ability, nameof(AbilityComponent.CooldownXpLeft),
                            valueType,
                            out var abilityXp)
                        && abilityXp.HasValue)
                    {
                        return (change.Entity.Id, true);
                    }

                    return (0, false);
                },
                new PropertyMatcher<GameEntity>()
                    .With(component => ((AbilityComponent)component).CooldownXpLeft, (int)EntityComponent.Ability)
            )
        );

        KnowledgeSystem = new KnowledgeSystem();
        queue.Register<VisibleTerrainChangedMessage>(KnowledgeSystem, VisibleTerrainChangedMessage.Name, 0);
        queue.Register<TraveledMessage>(KnowledgeSystem, TraveledMessage.Name, 4);
        queue.Register<ItemMovedMessage>(KnowledgeSystem, ItemMovedMessage.Name, 0);
        queue.Register<AttackResultMessage>(KnowledgeSystem, AttackResultMessage.Name, 1);
        queue.Register<SoundMessage>(KnowledgeSystem, SoundMessage.Name, 0);

        XPSystem = new XPSystem();
        queue.Register<KnownTerrainChangedMessage>(XPSystem, KnownTerrainChangedMessage.Name, 1);
        queue.Register<DiedMessage>(XPSystem, DiedMessage.Name, 3);

        LoggingSystem = new LoggingSystem();
        queue.Register<ItemMovedMessage>(LoggingSystem, ItemMovedMessage.Name, 1);
        queue.Register<ItemEquippedMessage>(LoggingSystem, ItemEquippedMessage.Name, 1);
        queue.Register<AttackResultMessage>(LoggingSystem, AttackResultMessage.Name, 2);
        queue.Register<ItemActivationResultMessage>(LoggingSystem, ItemActivationResultMessage.Name, 2);
        queue.Register<DiedMessage>(LoggingSystem, DiedMessage.Name, 1);
    }
}
