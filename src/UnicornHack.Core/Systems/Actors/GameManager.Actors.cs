using System.Collections.Generic;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public EntityGroup<GameEntity> Players
    {
        get;
        private set;
    }

    public EntityGroup<GameEntity> LevelActors
    {
        get;
        private set;
    }

    public LookupEntityRelationship<GameEntity, Point, Dictionary<Point, GameEntity>>
        LevelActorsToLevelCellRelationship
    {
        get;
        private set;
    }

    public LookupEntityRelationship<GameEntity, int, Dictionary<int, GameEntity>>
        SlottedAbilitiesToLevelActorsRelationship
    {
        get;
        private set;
    }

    public PlayerSystem PlayerSystem
    {
        get;
        private set;
    }

    public AISystem AISystem
    {
        get;
        private set;
    }

    private void InitializeActors(SequentialMessageQueue<GameManager> queue)
    {
        Add<PlayerComponent>(EntityComponent.Player, poolSize: 1);
        Add<AIComponent>(EntityComponent.AI, poolSize: 32);

        Players = CreateGroup(nameof(Players),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Player));

        LevelActors = CreateGroup(nameof(LevelActors),
            new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position, (int)EntityComponent.Being)
                .AnyOf((int)EntityComponent.AI, (int)EntityComponent.Player));

        LevelActorsToLevelCellRelationship = new(
            nameof(LevelActorsToLevelCellRelationship),
            LevelActors,
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
            (actorEntity, _) => actorEntity.RemoveComponent(EntityComponent.Position),
            levelEntity => (Dictionary<Point, GameEntity>)levelEntity.Level.Actors);

        SlottedAbilitiesToLevelActorsRelationship = new(
            nameof(SlottedAbilitiesToLevelActorsRelationship),
            Abilities,
            LevelActors,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((AbilityComponent)component).OwnerId,
                (int)EntityComponent.Ability),
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((AbilityComponent)component).Slot,
                (int)EntityComponent.Ability),
            (abilityEntity, _) => abilityEntity.RemoveComponent(EntityComponent.Ability),
            actorEntity => (Dictionary<int, GameEntity>)actorEntity.Being.SlottedAbilities);

        AISystem = new AISystem();
        queue.Register<PerformActionMessage>(AISystem, PerformActionMessage.AIName, 0);
        queue.Register<DecideNextActionMessage>(AISystem, DecideNextActionMessage.Name, 0);
        queue.Register<DelayMessage>(AISystem, DelayMessage.Name, 0);
        queue.Register<DiedMessage>(AISystem, DiedMessage.Name, 4);
        queue.Register<EntityAddedMessage<GameEntity>>(AISystem,
            AbilitiesToAffectableRelationship.Dependents.GetEntityAddedMessageName(),
            1);
        queue.Register<PropertyValueChangedMessage<GameEntity, bool>>(AISystem,
            AbilitiesToAffectableRelationship.Dependents.GetPropertyValueChangedMessageName(
                nameof(AbilityComponent.IsUsable)),
            1);

        PlayerSystem = new PlayerSystem();
        queue.Register<PerformActionMessage>(PlayerSystem, PerformActionMessage.PlayerName, 0);
        queue.Register<DelayMessage>(PlayerSystem, DelayMessage.Name, 1);
        queue.Register<DiedMessage>(PlayerSystem, DiedMessage.Name, 5);
        queue.Register<TraveledMessage>(PlayerSystem, TraveledMessage.Name, 1);
    }
}
