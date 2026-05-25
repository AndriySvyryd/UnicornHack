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

    private void InitializeKnowledge(SequentialMessageQueue<GameManager> queue)
    {
        Add<KnowledgeComponent>(EntityComponent.Knowledge, poolSize: 32);

        KnownPositions = CreateGroup(nameof(KnownPositions),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Knowledge, (int)EntityComponent.Position));

        KnownActorsToCellRelationship = new(
            nameof(KnownActorsToCellRelationship),
            KnownPositions,
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
            (knowledgeEntity, _) =>
            {
                knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                knowledgeEntity.RemoveComponent(EntityComponent.Position);
            },
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.KnownActors,
            secondaryPrincipalGroup: RoomActors,
            secondaryPrincipalKeyValueGetter: new SimpleKeyValueGetter<GameEntity, int>(
                component => ((KnowledgeComponent)component).KnownEntityId,
                (int)EntityComponent.Knowledge));

        KnownItemsToCellRelationship = new(
            nameof(KnownItemsToCellRelationship),
            KnownPositions,
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
            (knowledgeEntity, _) =>
            {
                knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                knowledgeEntity.RemoveComponent(EntityComponent.Position);
            },
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.KnownItems,
            secondaryPrincipalGroup: RoomItems,
            secondaryPrincipalKeyValueGetter: new SimpleKeyValueGetter<GameEntity, int>(
                component => ((KnowledgeComponent)component).KnownEntityId,
                (int)EntityComponent.Knowledge));

        KnownConnectionsToCellRelationship = new(
            nameof(KnownConnectionsToCellRelationship),
            KnownPositions,
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
            (knowledgeEntity, _) =>
            {
                knowledgeEntity.RemoveComponent(EntityComponent.Knowledge);
                knowledgeEntity.RemoveComponent(EntityComponent.Position);
            },
            roomEntity => (SortedDictionary<Point, GameEntity>)roomEntity.Room!.KnownConnections,
            secondaryPrincipalGroup: Connections,
            secondaryPrincipalKeyValueGetter: new SimpleKeyValueGetter<GameEntity, int>(
                component => ((KnowledgeComponent)component).KnownEntityId,
                (int)EntityComponent.Knowledge));

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

                    if (matcher.TryGetValue<int?>(
                            change, (int)EntityComponent.Effect, nameof(EffectComponent.ExpirationXp), valueType,
                            out var effectXp)
                        && effectXp.HasValue)
                    {
                        return (change.Entity.Id, true);
                    }

                    return (0, false);
                },
                new PropertyMatcher<GameEntity>()
                    .With(component => ((EffectComponent)component).ExpirationXp, (int)EntityComponent.Effect)
                    .With(component => ((AbilityComponent)component).CooldownXpLeft, (int)EntityComponent.Ability)
            )
        );

        KnowledgeSystem = new KnowledgeSystem();
        queue.Register<VisibleTerrainChangedMessage>(KnowledgeSystem, VisibleTerrainChangedMessage.Name, 0);
        queue.Register<TraveledMessage>(KnowledgeSystem, TraveledMessage.Name, 4);
        queue.Register<ItemMovedMessage>(KnowledgeSystem, ItemMovedMessage.Name, 0);
        queue.Register<EffectsAppliedMessage>(KnowledgeSystem, EffectsAppliedMessage.Name, 1);

        XPSystem = new XPSystem();
        queue.Register<KnownTerrainChangedMessage>(XPSystem, KnownTerrainChangedMessage.Name, 1);
        queue.Register<DiedMessage>(XPSystem, DiedMessage.Name, 3);
        queue.Register<EntityAddedMessage<GameEntity>>(XPSystem, Races.GetEntityAddedMessageName(), 0);
        queue.Register<EntityRemovedMessage<GameEntity>>(XPSystem, Races.GetEntityRemovedMessageName(), 0);

        LoggingSystem = new LoggingSystem();
        queue.Register<ItemMovedMessage>(LoggingSystem, ItemMovedMessage.Name, 1);
        queue.Register<ItemEquippedMessage>(LoggingSystem, ItemEquippedMessage.Name, 1);
        queue.Register<EffectsAppliedMessage>(LoggingSystem, EffectsAppliedMessage.Name, 2);
        queue.Register<DiedMessage>(LoggingSystem, DiedMessage.Name, 1);
        queue.Register<LeveledUpMessage>(LoggingSystem, LeveledUpMessage.Name, 1);
    }
}
