using BenchmarkDotNet.Attributes;
using UnicornHack.Systems.Levels;

namespace UnicornHack.PerformanceTests.Utils.MessagingECS
{
    public class EntityRelationshipPerfTest
    {
        public int EntityCount = 1000000;

        private GameEntity _levelEntity;
        private ConnectionComponent[] _connections;

        [GlobalSetup(Target = nameof(UpdateRelationship))]
        public void RelationshipSetup()
        {
            var manager = TestHelper.CreateGameManager();

            if (_connections == null
                || _connections.Length != EntityCount)
            {
                _connections = new ConnectionComponent[EntityCount];
            }

            _levelEntity = manager.CreateEntity().Referenced;
            _levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

            for (var i = 0; i < EntityCount; i++)
            {
                var effectEntity = manager.CreateEntity().Referenced;
                _connections[i] = effectEntity.AddComponent<ConnectionComponent>((int)EntityComponent.Connection);
            }
        }

        [GlobalCleanup(Target = nameof(UpdateRelationship))]
        public void RelationshipCleanup()
        {
            var context = _levelEntity.Manager;
            _levelEntity.RemoveReference(context);
            for (var i = 0; i < EntityCount; i++)
            {
                _connections[i].Entity.RemoveReference(context);
            }
        }

        [Benchmark]
        public void UpdateRelationship()
        {
            for (var i = 0; i < EntityCount; i++)
            {
                _connections[i].TargetLevelId = _levelEntity.Id;
            }
        }
    }
}
