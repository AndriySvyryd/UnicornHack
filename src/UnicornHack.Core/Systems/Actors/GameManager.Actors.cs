using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> Players { get; private set; }
        public EntityGroup<GameEntity> LevelActors { get; private set; }
        public UniqueEntityIndex<GameEntity, (int, byte, byte)> LevelActorToLevelCellIndex { get; private set; }
        public EntityRelationship<GameEntity> LevelActorsToLevelRelationship { get; private set; }
        public PlayerSystem PlayerSystem { get; private set; }
        public AISystem AISystem { get; private set; }

        private void InitializeActors(SequentialMessageQueue<GameManager> queue)
        {
            Add<PlayerComponent>(EntityComponent.Player, poolSize: 1);
            Add<AIComponent>(EntityComponent.AI, poolSize: 32);

            Players = CreateGroup(nameof(Players),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Player));

            LevelActors = CreateGroup(nameof(LevelActors),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position)
                    .AnyOf((int)EntityComponent.AI, (int)EntityComponent.Player));

            LevelActorToLevelCellIndex = new UniqueEntityIndex<GameEntity, (int, byte, byte)>(
                nameof(LevelActorToLevelCellIndex),
                LevelActors,
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

            LevelActorsToLevelRelationship = new EntityRelationship<GameEntity>(
                nameof(LevelActorsToLevelRelationship),
                LevelActors,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((PositionComponent)component).LevelId,
                    (int)EntityComponent.Position),
                (effectEntity, _, __) => effectEntity.RemoveComponent(EntityComponent.Position));

            AISystem = new AISystem();
            queue.Add<PerformActionMessage>(AISystem, PerformActionMessage.AIName, 0);
            queue.Add<DelayMessage>(AISystem, DelayMessage.Name, 0);
            queue.Add<DiedMessage>(AISystem, DiedMessage.Name, 4);
            queue.Add<EntityAddedMessage<GameEntity>>(AISystem,
                AbilitiesToAffectableRelationship.GetEntityAddedMessageName(),
                1);
            queue.Add<PropertyValueChangedMessage<GameEntity, bool>>(AISystem,
                AbilitiesToAffectableRelationship.GetPropertyValueChangedMessageName(nameof(AbilityComponent.IsUsable)),
                1);

            PlayerSystem = new PlayerSystem();
            queue.Add<PerformActionMessage>(PlayerSystem, PerformActionMessage.PlayerName, 0);
            queue.Add<DelayMessage>(PlayerSystem, DelayMessage.Name, 1);
            queue.Add<DiedMessage>(PlayerSystem, DiedMessage.Name, 5);
            queue.Add<TraveledMessage>(PlayerSystem, TraveledMessage.Name, 1);
        }
    }
}
