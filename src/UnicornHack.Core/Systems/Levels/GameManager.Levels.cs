using System.Collections.Generic;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> Levels { get; private set; }
        public EntityGroup<GameEntity> Connections { get; private set; }
        public EntityGroup<GameEntity> LevelEntities { get; private set; }
        public LookupEntityRelationship<GameEntity, Point, Dictionary<Point, GameEntity>> ConnectionsToLevelCellRelationship { get; private set; }
        public CollectionEntityRelationship<GameEntity, HashSet<GameEntity>> IncomingConnectionsToLevelRelationship { get; private set; }
        public TravelSystem TravelSystem { get; private set; }

        private void InitializeLevels(SequentialMessageQueue<GameManager> queue)
        {
            Add<LevelComponent>(EntityComponent.Level, poolSize: 4);
            Add<ConnectionComponent>(EntityComponent.Connection, poolSize: 8);
            Add<PositionComponent>(EntityComponent.Position, poolSize: 32);

            Levels = CreateGroup(nameof(Levels),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Level));
            Connections = CreateGroup(nameof(Connections),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position, (int)EntityComponent.Connection));
            LevelEntities = CreateGroup(nameof(LevelEntities),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position));

            ConnectionsToLevelCellRelationship = new(
                nameof(ConnectionsToLevelCellRelationship),
                Connections,
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
                (connectionEntity, _) => connectionEntity.RemoveComponent(EntityComponent.Position),
                levelEntity => (Dictionary<Point, GameEntity>)levelEntity.Level.Connections);

            IncomingConnectionsToLevelRelationship = new(
                nameof(IncomingConnectionsToLevelRelationship),
                Connections,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((ConnectionComponent)component).TargetLevelId,
                    (int)EntityComponent.Connection),
                (effectEntity, _) => effectEntity.RemoveComponent(EntityComponent.Position),
                levelEntity => (HashSet<GameEntity>)levelEntity.Level.IncomingConnections);

            TravelSystem = new TravelSystem();
            queue.Register<TravelMessage>(TravelSystem, TravelMessage.Name, 0);
            queue.Register<DiedMessage>(TravelSystem, DiedMessage.Name, 6);
        }
    }
}
