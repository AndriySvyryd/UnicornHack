using System;
using BenchmarkDotNet.Attributes;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.PerformanceTests.Utils.MessagingECS
{
    public class EntityRelationshipPerfTest
    {
        public int EntityCount = 10000;

        private GameEntity _levelEntity;
        private ConnectionComponent[] _connections;

        [GlobalSetup(Target = nameof(CollectionAdd))]
        public void CollectionSetup()
        {
            var manager = TestHelper.CreateGameManager();
            var testSystem = new RelationshipTestSystem();
            manager.IncomingConnectionsToLevelRelationship.Dependents.AddListener(testSystem);

            if (_connections == null
                || _connections.Length != EntityCount)
            {
                _connections = new ConnectionComponent[EntityCount];
            }

            _levelEntity = manager.CreateEntity().Referenced;
            var level = _levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

            level.Height = (byte)Math.Ceiling(Math.Sqrt(EntityCount));
            level.Width = level.Height;

            byte x = 0;
            byte y = 0;
            for (var i = 0; i < EntityCount; i++)
            {
                var connectionEntity = manager.CreateEntity().Referenced;

                var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
                position.LevelEntity = _levelEntity;
                position.LevelCell = new Point(x++, y);
                if (x == level.Width)
                {
                    x = 0;
                    y++;
                }

                connectionEntity.Position = position;

                var connection = connectionEntity.AddComponent<ConnectionComponent>((int)EntityComponent.Connection);
                _connections[i] = connection;
            }

            manager.Queue.ProcessQueue(manager);
            manager.Queue.Suspend();
        }

        [GlobalSetup(Targets = new[] {nameof(CollectionRemove), nameof(CollectionUpdate), nameof(CollectionCompositeUpdate) })]
        public void InitializedCollectionSetup()
        {
            CollectionSetup();
            CollectionAdd();
        }

        private class RelationshipTestSystem :
            IGameSystem<EntityAddedMessage<GameEntity>>,
            IGameSystem<EntityRemovedMessage<GameEntity>>,
            IGameSystem<PropertyValueChangedMessage<GameEntity, string>>,
            IEntityChangeListener<GameEntity>
        {
            public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager state)
                => MessageProcessingResult.ContinueProcessing;

            public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager state)
                => MessageProcessingResult.ContinueProcessing;

            public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, string> message, GameManager state)
                => MessageProcessingResult.ContinueProcessing;

            public void OnEntityAdded(in EntityChange<GameEntity> entityChange)
            {
            }

            public void OnEntityRemoved(in EntityChange<GameEntity> entityChange)
            {
            }

            public bool OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
                => false;
        }

        [Benchmark]
        public void CollectionAdd()
        {
            for (var i = 0; i < EntityCount; i++)
            {
                _connections[i].TargetLevelId = _levelEntity.Id;
            }
        }

        [Benchmark]
        public void CollectionRemove()
        {
            for (var i = 0; i < EntityCount; i++)
            {
                _connections[i].TargetLevelId = 0;
            }
        }

        [Benchmark]
        public void CollectionUpdate()
        {
            for (var i = 0; i < EntityCount; i++)
            {
                _connections[i].TargetLevelX = 1;
                _connections[i].TargetLevelY = 1;
            }
        }

        [Benchmark]
        public void CollectionCompositeUpdate()
        {
            for (var i = 0; i < EntityCount; i++)
            {
                _connections[i].TargetLevelCell = new Point(1, 1);
            }
        }
    }
}
