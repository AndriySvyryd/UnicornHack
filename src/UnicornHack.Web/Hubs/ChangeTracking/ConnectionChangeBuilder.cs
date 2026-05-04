using System.Collections;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class ConnectionChangeBuilder : ChangeBuilder<ConnectionChange>
{
    public override void RegisterOnGroups(GameManager manager)
    {
        manager.KnownPositions.AddListener(this);
        manager.KnownConnectionsToLevelCellRelationship.AddDependentsListener(this);
    }

    public override void UnregisterFromGroups(GameManager manager)
    {
        manager.KnownPositions.RemoveListener(this);
        manager.KnownConnectionsToLevelCellRelationship.RemoveDependentsListener(this);
    }

    protected override bool IsRelevant(GameEntity entity)
        => IsSourceConnection(entity.Knowledge?.KnownEntity.Connection);

    protected override bool IsRelevantOnRemove(in EntityChange<GameEntity> entityChange)
        => entityChange.RemovedComponent is KnowledgeComponent knowledge
            && IsSourceConnection(knowledge.KnownEntity.Connection);

    protected override ConnectionChange ProduceFullSnapshot(GameEntity entity, SerializationContext context)
    {
        var snapshot = SerializeConnection(entity);
        snapshot.ChangedProperties = null;
        return snapshot;
    }

    protected override ConnectionChange? ProduceDelta(GameEntity entity, ConnectionChange tracked, SerializationContext context)
    {
        if (entity.Knowledge?.KnownEntity.Connection == null)
        {
            // Stale reference — emit a removal sentinel.
            return new ConnectionChange { ChangedProperties = new BitArray(1) };
        }

        return SerializeConnection(entity);
    }

    public static ConnectionChange SerializeConnection(GameEntity connectionEntity)
    {
        var knownEntity = connectionEntity.Knowledge!.KnownEntity;
        var position = knownEntity.Position!;
        return new ConnectionChange
        {
            LevelX = position.LevelX,
            LevelY = position.LevelY,
            IsDown = knownEntity.Connection!.TargetLevelEntity.Level!.Depth
                > position.LevelEntity.Level!.Depth
        };
    }

    private static bool IsSourceConnection(ConnectionComponent? connection)
        => connection != null
            && (connection.Direction == null
                || (connection.Direction & ConnectionDirection.Source) != 0);
}
