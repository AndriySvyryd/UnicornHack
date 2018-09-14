using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> Levels { get; private set; }
        public EntityGroup<GameEntity> Connections { get; private set; }
        public EntityGroup<GameEntity> LevelEntities { get; private set; }
        public EntityRelationship<GameEntity> ConnectionsToLevelRelationship { get; private set; }
        public EntityRelationship<GameEntity> IncomingConnectionsToLevelRelationship { get; private set; }
        public TravelSystem TravelSystem { get; private set; }

        private void InitializeLevels(SequentialMessageQueue<GameManager> queue)
        {
            Add<LevelComponent>(EntityComponent.Level, poolSize: 2);
            Add<ConnectionComponent>(EntityComponent.Connection, poolSize: 2);
            Add<PositionComponent>(EntityComponent.Position, poolSize: 32);

            Levels = CreateGroup(nameof(Levels),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Level));
            Connections = CreateGroup(nameof(Connections),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position, (int)EntityComponent.Connection));
            LevelEntities = CreateGroup(nameof(LevelEntities),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Position));

            ConnectionsToLevelRelationship = new EntityRelationship<GameEntity>(
                nameof(ConnectionsToLevelRelationship),
                Connections,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((PositionComponent)component).LevelId,
                    (int)EntityComponent.Position),
                (effectEntity, _, __) => effectEntity.RemoveComponent(EntityComponent.Position));

            IncomingConnectionsToLevelRelationship = new EntityRelationship<GameEntity>(
                nameof(IncomingConnectionsToLevelRelationship),
                Connections,
                Levels,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((ConnectionComponent)component).TargetLevelId,
                    (int)EntityComponent.Connection),
                (effectEntity, _, __) => effectEntity.RemoveComponent(EntityComponent.Position));

            TravelSystem = new TravelSystem();
            queue.Add<TravelMessage>(TravelSystem, TravelSystem.TravelMessageName, 0);
            queue.Add<DiedMessage>(TravelSystem, LivingSystem.DiedMessageName, 2);
        }
    }
}
