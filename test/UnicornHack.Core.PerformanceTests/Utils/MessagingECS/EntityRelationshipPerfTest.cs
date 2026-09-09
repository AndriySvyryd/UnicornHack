using BenchmarkDotNet.Attributes;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.PerformanceTests.Utils.MessagingECS;

public class EntityRelationshipPerfTest
{
    public int EntityCount = 10000;

    private GameEntity _levelEntity = null!;
    private ConnectionComponent[] _connections = null!;

    [GlobalSetup]
    public void CollectionSetup()
    {
        var manager = TestHelper.CreateGameManager();
        var testSystem = new RelationshipTestSystem();
        manager.IncomingConnectionsToRoomRelationship.Dependents.AddListener(testSystem);

        _connections = new ConnectionComponent[EntityCount];

        _levelEntity = manager.CreateEntity().Referenced;
        var level = _levelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);

        level.Height = (byte)Math.Ceiling(Math.Sqrt(EntityCount));
        level.Width = level.Height;

        byte x = 0;
        byte y = 0;
        for (var i = 0; i < EntityCount; i++)
        {
            var connectionEntity = manager.CreateEntity().Referenced;

            var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
            position.RoomEntity = _levelEntity;
            position.Cell = new Point(x++, y);
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

    [IterationSetup(Target = nameof(CollectionAdd))]
    public void ResetCollectionAdd()
    {
        foreach (var connection in _connections)
        {
            connection.TargetRoomId = 0;
        }
    }

    [IterationSetup(Target = nameof(CollectionRemove))]
    public void ResetCollectionRemove()
    {
        foreach (var connection in _connections)
        {
            connection.TargetRoomId = _levelEntity.Id;
        }
    }

    [IterationSetup(Target = nameof(CollectionUpdate))]
    public void ResetCollectionUpdate()
    {
        foreach (var connection in _connections)
        {
            connection.TargetRoomId = _levelEntity.Id;
            connection.TargetLevelX = 0;
            connection.TargetLevelY = 0;
        }
    }

    [IterationSetup(Target = nameof(CollectionCompositeUpdate))]
    public void ResetCollectionCompositeUpdate()
    {
        foreach (var connection in _connections)
        {
            connection.TargetRoomId = _levelEntity.Id;
            connection.TargetCell = default;
        }
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

        public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, string> message,
            GameManager state)
            => MessageProcessingResult.ContinueProcessing;

        public void OnEntityAdded(in EntityChange<GameEntity> entityChange)
        {
        }

        public void OnEntityRemoved(in EntityChange<GameEntity> entityChange)
        {
        }

        public void OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
        {
        }
    }

    [Benchmark]
    public void CollectionAdd()
    {
        for (var i = 0; i < EntityCount; i++)
        {
            _connections[i].TargetRoomId = _levelEntity.Id;
        }
    }

    [Benchmark]
    public void CollectionRemove()
    {
        for (var i = 0; i < EntityCount; i++)
        {
            _connections[i].TargetRoomId = 0;
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
            _connections[i].TargetCell = new Point(1, 1);
        }
    }
}
