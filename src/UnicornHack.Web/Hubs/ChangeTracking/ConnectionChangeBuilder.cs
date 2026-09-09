using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class ConnectionChangeBuilder
    : IEntityChangeListener<GameEntity>, IDependentEntityChangeListener<GameEntity>
{
    private const uint PositionDirty = 1 << 0;
    private const uint MembershipDirty = 1 << 1;

    private readonly record struct PendingChange(
        GameEntity Entity,
        EntityState State,
        uint DirtyMask,
        int InitialRoomId,
        int FinalRoomId);

    private readonly record struct Projection(int RoomId, int LevelX, int LevelY, Direction Orientation);

    private readonly Dictionary<int, PendingChange> _pending = [];
    private readonly Dictionary<int, Projection> _projection = [];

    public bool HasChanges => _pending.Count != 0;

    public void RegisterOnGroups(GameManager manager)
    {
        manager.KnownPositions.AddListener(this);
        manager.KnownConnectionsToCellRelationship.AddDependentsListener(this);
        manager.Connections.AddListener(this);
    }

    public void UnregisterFromGroups(GameManager manager)
    {
        manager.KnownPositions.RemoveListener(this);
        manager.KnownConnectionsToCellRelationship.RemoveDependentsListener(this);
        manager.Connections.RemoveListener(this);
    }

    public bool HasChangesForRoom(int roomId)
    {
        foreach (var (id, pending) in _pending)
        {
            if (ShouldDelete(id, pending, roomId)
                || ShouldSerialize(id, pending, roomId))
            {
                return true;
            }
        }

        return false;
    }

    public Dictionary<int, ConnectionChange>? BuildChanges(int roomId)
    {
        var outputCount = 0;
        foreach (var (id, pending) in _pending)
        {
            if (ShouldDelete(id, pending, roomId)
                || ShouldSerialize(id, pending, roomId))
            {
                outputCount++;
            }
        }

        if (outputCount == 0)
        {
            return null;
        }

        var output = new Dictionary<int, ConnectionChange>(outputCount);
        foreach (var (id, pending) in _pending)
        {
            ConnectionChange? change = null;
            if (ShouldDelete(id, pending, roomId))
            {
                change = new ConnectionChange { ChangedProperties = 0 };
                _projection.Remove(id);
            }
            else if (ShouldSerialize(id, pending, roomId)
                     && TryGetCurrent(pending.Entity, out var current))
            {
                change = _projection.TryGetValue(id, out var previous)
                         && previous.RoomId == roomId
                    ? SerializeDelta(previous, current)
                    : SerializeFull(current);
                _projection[id] = current;
            }

            if (change != null)
            {
                output.Add(id, change);
            }
        }

        return output;
    }

    public void Reset() => _pending.Clear();

    public void ResetProjection(GameEntity playerEntity)
    {
        _projection.Clear();
        if (playerEntity.Position?.RoomEntity.Room is not { } room)
        {
            return;
        }

        foreach (var connectionEntity in room.KnownConnections.Values)
        {
            if (IsRelevant(connectionEntity)
                && TryGetCurrent(connectionEntity, out var current))
            {
                _projection[connectionEntity.Id] = current;
            }
        }
    }

    public static ConnectionChange SerializeConnection(GameEntity connectionEntity)
    {
        TryGetCurrent(connectionEntity, out var current);
        return new ConnectionChange
        {
            ChangedProperties = null,
            LevelX = current.LevelX,
            LevelY = current.LevelY,
            Orientation = current.Orientation
        };
    }

    void IDependentEntityChangeListener<GameEntity>.OnEntityAdded(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (IsRelevant(entityChange.Entity))
        {
            TrackAdded(entityChange.Entity, principal.Id);
        }
    }

    void IDependentEntityChangeListener<GameEntity>.OnEntityRemoved(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (!IsRelevant(entityChange.Entity)
            && !IsRelevantOnRemove(entityChange))
        {
            return;
        }

        var finalRoomId = HasRoomIdChange(entityChange.PropertyChanges)
            ? entityChange.Entity.Position?.RoomId ?? 0
            : 0;
        TrackRemoved(entityChange.Entity, principal.Id, finalRoomId);
    }

    void IDependentEntityChangeListener<GameEntity>.OnPropertyValuesChanged(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (_pending.ContainsKey(entityChange.Entity.Id)
            || IsRelevant(entityChange.Entity))
        {
            TrackModified(entityChange.Entity, principal.Id);
        }
    }

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var entity = entityChange.Entity;
        var knowledgeEntity = entity.Position?.Knowledge;
        if (entity.Connection == null
            || knowledgeEntity == null)
        {
            return;
        }

        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);
            if (componentId == (int)EntityComponent.Connection
                && propertyName == nameof(ConnectionComponent.Directionality))
            {
                var wasRelevant = IsSourceConnection(
                    changes.GetValue<Directionality?>(i, Utils.MessagingECS.ValueType.Old));
                var isRelevant = IsSourceConnection(entity.Connection.Directionality);
                if (wasRelevant != isRelevant)
                {
                    if (isRelevant)
                    {
                        TrackAdded(knowledgeEntity, knowledgeEntity.Position!.RoomId);
                    }
                    else
                    {
                        TrackRemoved(
                            knowledgeEntity,
                            knowledgeEntity.Position!.RoomId,
                            finalRoomId: 0);
                    }

                    return;
                }
            }

            if ((componentId == (int)EntityComponent.Connection
                 && propertyName == nameof(ConnectionComponent.Orientation))
                || (componentId == (int)EntityComponent.Position
                    && propertyName is nameof(PositionComponent.CellX) or nameof(PositionComponent.CellY)))
            {
                if (IsRelevant(knowledgeEntity))
                {
                    TrackModified(knowledgeEntity, knowledgeEntity.Position!.RoomId);
                }

                return;
            }
        }
    }

    private void TrackAdded(GameEntity entity, int roomId)
    {
        if (_pending.TryGetValue(entity.Id, out var pending))
        {
            _pending[entity.Id] = pending with
            {
                Entity = entity,
                State = _projection.ContainsKey(entity.Id) ? EntityState.Modified : EntityState.Added,
                DirtyMask = pending.DirtyMask | MembershipDirty,
                FinalRoomId = roomId
            };
            return;
        }

        _pending[entity.Id] = new PendingChange(
            entity,
            _projection.ContainsKey(entity.Id) ? EntityState.Modified : EntityState.Added,
            MembershipDirty,
            InitialRoomId: 0,
            FinalRoomId: roomId);
    }

    private void TrackRemoved(GameEntity entity, int initialRoomId, int finalRoomId)
    {
        if (_pending.TryGetValue(entity.Id, out var pending))
        {
            if (pending.State == EntityState.Added
                && !_projection.ContainsKey(entity.Id)
                && finalRoomId == 0)
            {
                _pending.Remove(entity.Id);
                return;
            }

            _pending[entity.Id] = pending with
            {
                Entity = entity,
                State = EntityState.Deleted,
                DirtyMask = pending.DirtyMask | MembershipDirty,
                FinalRoomId = finalRoomId
            };
            return;
        }

        if (_projection.ContainsKey(entity.Id))
        {
            _pending[entity.Id] = new PendingChange(
                entity,
                EntityState.Deleted,
                MembershipDirty,
                initialRoomId,
                finalRoomId);
        }
    }

    private void TrackModified(GameEntity entity, int roomId)
    {
        if (_pending.TryGetValue(entity.Id, out var pending))
        {
            _pending[entity.Id] = pending with
            {
                Entity = entity,
                DirtyMask = pending.DirtyMask | PositionDirty,
                FinalRoomId = roomId
            };
            return;
        }

        _pending[entity.Id] = new PendingChange(
            entity,
            _projection.ContainsKey(entity.Id) ? EntityState.Modified : EntityState.Added,
            PositionDirty,
            roomId,
            roomId);
    }

    private bool ShouldDelete(int id, PendingChange pending, int roomId)
        => pending.InitialRoomId == roomId
            && pending.FinalRoomId != roomId
            && _projection.TryGetValue(id, out var previous)
            && previous.RoomId == roomId;

    private bool ShouldSerialize(int id, PendingChange pending, int roomId)
    {
        if (pending.FinalRoomId != roomId)
        {
            return false;
        }

         return !_projection.TryGetValue(id, out var previous)
             || previous.RoomId != roomId
               || TryGetCurrent(pending.Entity, out var current) && current != previous;
    }

    private static bool TryGetCurrent(GameEntity entity, out Projection projection)
    {
        if (entity.Knowledge?.KnownEntity is not { Position: { } position, Connection: { } connection })
        {
            projection = default;
            return false;
        }

        projection = new Projection(
            position.RoomId,
            position.CellX!.Value,
            position.CellY!.Value,
            connection.Orientation);
        return true;
    }

    private static ConnectionChange? SerializeDelta(Projection previous, Projection current)
    {
        var change = new ConnectionChange();
        if (current.LevelX != previous.LevelX)
        {
            change.LevelX = current.LevelX;
        }
        if (current.LevelY != previous.LevelY)
        {
            change.LevelY = current.LevelY;
        }
        if (current.Orientation != previous.Orientation)
        {
            change.Orientation = current.Orientation;
        }

        var bits = change.ChangedProperties;
        return bits.IsPropertyChanged(1)
               || bits.IsPropertyChanged(2)
               || bits.IsPropertyChanged(3)
            ? change
            : null;
    }

    private static ConnectionChange SerializeFull(Projection current)
        => new()
        {
            ChangedProperties = null,
            LevelX = current.LevelX,
            LevelY = current.LevelY,
            Orientation = current.Orientation
        };

    private static bool IsRelevant(GameEntity entity)
        => IsSourceConnection(entity.Knowledge?.KnownEntity.Connection);

    private static bool IsRelevantOnRemove(in EntityChange<GameEntity> entityChange)
        => entityChange.RemovedComponent is KnowledgeComponent knowledge
            && IsSourceConnection(knowledge.KnownEntity.Connection);

    private static bool IsSourceConnection(ConnectionComponent? connection)
        => connection != null && IsSourceConnection(connection.Directionality);

    private static bool IsSourceConnection(Directionality? directionality)
        => directionality == null
            || (directionality & Directionality.Source) != 0;

    private static bool HasRoomIdChange(IPropertyValueChanges changes)
    {
        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId == (int)EntityComponent.Position
                && changes.GetChangedPropertyName(i) == nameof(PositionComponent.RoomId))
            {
                return true;
            }
        }

        return false;
    }
}
