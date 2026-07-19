using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class ConnectionChangeBuilder : ChangeBuilder<ConnectionChange>
{
    public override void RegisterOnGroups(GameManager manager)
    {
        manager.KnownPositions.AddListener(this);
        manager.KnownConnectionsToCellRelationship.AddDependentsListener(this);
    }

    public override void UnregisterFromGroups(GameManager manager)
    {
        manager.KnownPositions.RemoveListener(this);
        manager.KnownConnectionsToCellRelationship.RemoveDependentsListener(this);
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
            return new ConnectionChange { ChangedProperties = SerializationContext.DeletedBitArray };
        }

        return SerializeConnection(entity);
    }

    public static ConnectionChange SerializeConnection(GameEntity connectionEntity)
    {
        var knownEntity = connectionEntity.Knowledge!.KnownEntity;
        var position = knownEntity.Position!;
        return new ConnectionChange
        {
            LevelX = position.CellX!.Value,
            LevelY = position.CellY!.Value,
            Orientation = knownEntity.Connection!.Orientation
        };
    }

    private static bool IsSourceConnection(ConnectionComponent? connection)
        => connection != null
            && (connection.Directionality == null
                || (connection.Directionality & Directionality.Source) != 0);
}
