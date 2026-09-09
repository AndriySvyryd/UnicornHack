using UnicornHack.Services;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class ItemChangeBuilder
    : IEntityChangeListener<GameEntity>, IDependentEntityChangeListener<GameEntity>
{
    private const uint PositionDirty = 1 << 0;
    private const uint IdentityDirty = 1 << 1;
    private const uint PerceptionDirty = 1 << 2;
    private const uint MembershipDirty = 1 << 3;
    private const uint AllValuesDirty = PositionDirty | IdentityDirty | PerceptionDirty;

    private readonly record struct PendingChange(
        GameEntity Entity,
        EntityState State,
        uint DirtyMask,
        int InitialRoomId,
        int FinalRoomId);

    private readonly record struct Projection(
        int RoomId,
        int LevelX,
        int LevelY,
        ItemType Type,
        string? BaseName,
        string? Name,
        bool IsCurrentlyPerceived);

    private readonly Dictionary<int, PendingChange> _pending = [];
    private readonly Dictionary<int, Projection> _projection = [];
    private readonly Dictionary<int, Projection> _prepared = [];

    public bool HasChanges => _pending.Count != 0;

    public void RegisterOnGroups(GameManager manager)
    {
        manager.KnownPositions.AddListener(this);
        manager.KnownItemsToCellRelationship.AddDependentsListener(this);
        manager.RoomItems.AddListener(this);
    }

    public void UnregisterFromGroups(GameManager manager)
    {
        manager.KnownPositions.RemoveListener(this);
        manager.KnownItemsToCellRelationship.RemoveDependentsListener(this);
        manager.RoomItems.RemoveListener(this);
    }

    public bool HasChangesForRoom(int roomId, GameServices services)
    {
        foreach (var (id, pending) in _pending)
        {
            if (ShouldDelete(id, pending, roomId)
                || TryPrepareChangedProjection(id, pending, roomId, services, out _))
            {
                return true;
            }
        }

        return false;
    }

    public Dictionary<int, ItemChange>? BuildChanges(int roomId, GameServices services)
    {
        var outputCount = 0;
        foreach (var (id, pending) in _pending)
        {
            if (ShouldDelete(id, pending, roomId)
                || TryPrepareChangedProjection(id, pending, roomId, services, out _))
            {
                outputCount++;
            }
        }

        if (outputCount == 0)
        {
            return null;
        }

        var output = new Dictionary<int, ItemChange>(outputCount);
        foreach (var (id, pending) in _pending)
        {
            ItemChange? change = null;
            if (ShouldDelete(id, pending, roomId))
            {
                change = new ItemChange { ChangedProperties = 0 };
                _projection.Remove(id);
            }
            else if (TryPrepareChangedProjection(id, pending, roomId, services, out var current))
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

    public void Reset()
    {
        _pending.Clear();
        _prepared.Clear();
    }

    public void ResetProjection(GameEntity playerEntity)
    {
        _projection.Clear();
        if (playerEntity.Position?.RoomEntity.Room is not { } room)
        {
            return;
        }

        foreach (var itemEntity in room.KnownItems.Values)
        {
                if (TryGetCurrent(
                    itemEntity, playerEntity.Game.Services, AllValuesDirty, previous: null, out var current))
            {
                _projection[itemEntity.Id] = current;
            }
        }
    }

    public static ItemChange SerializeItem(
        GameEntity knowledgeEntity, in SerializationContext context)
    {
        TryGetCurrent(knowledgeEntity, context.Services, AllValuesDirty, previous: null, out var current);
        return SerializeFull(current);
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
            TrackModified(
                entityChange.Entity,
                principal.Id,
                GetDirtyMask(entityChange.PropertyChanges));
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
        if (entity.Item != null)
        {
            if (entity.Position?.Knowledge is { } knowledgeEntity)
            {
                TrackModified(
                    knowledgeEntity,
                    knowledgeEntity.Position!.RoomId,
                    GetDirtyMask(entityChange.PropertyChanges));
            }

            return;
        }

        if (IsRelevant(entity))
        {
            TrackModified(entity, entity.Position!.RoomId, GetDirtyMask(entityChange.PropertyChanges));
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
                DirtyMask = pending.DirtyMask | MembershipDirty | AllValuesDirty,
                FinalRoomId = roomId
            };
            return;
        }

        _pending[entity.Id] = new PendingChange(
            entity,
            _projection.ContainsKey(entity.Id) ? EntityState.Modified : EntityState.Added,
            MembershipDirty | AllValuesDirty,
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

    private void TrackModified(GameEntity entity, int roomId, uint dirtyMask)
    {
        if (dirtyMask == 0)
        {
            return;
        }

        if (_pending.TryGetValue(entity.Id, out var pending))
        {
            _pending[entity.Id] = pending with
            {
                Entity = entity,
                DirtyMask = pending.DirtyMask | dirtyMask,
                FinalRoomId = pending.State == EntityState.Deleted
                    ? pending.FinalRoomId
                    : roomId
            };
            return;
        }

        _pending[entity.Id] = new PendingChange(
            entity,
            _projection.ContainsKey(entity.Id) ? EntityState.Modified : EntityState.Added,
            dirtyMask,
            roomId,
            roomId);
    }

    private bool ShouldDelete(int id, PendingChange pending, int roomId)
        => pending.InitialRoomId == roomId
            && pending.FinalRoomId != roomId
            && _projection.TryGetValue(id, out var previous)
            && previous.RoomId == roomId;

    private bool TryPrepareChangedProjection(
        int id,
        PendingChange pending,
        int roomId,
        GameServices services,
        out Projection current)
    {
        if (pending.FinalRoomId != roomId)
        {
            current = default;
            return false;
        }

        if (_prepared.TryGetValue(id, out current))
        {
            return true;
        }

        var hasPrevious = _projection.TryGetValue(id, out var previous)
                          && previous.RoomId == roomId;
        if (!TryGetCurrent(
                pending.Entity,
                services,
                pending.DirtyMask,
                hasPrevious ? previous : null,
                out current)
            || hasPrevious && current == previous)
        {
            return false;
        }

        _prepared[id] = current;
        return true;
    }

    private static bool TryGetCurrent(
        GameEntity entity,
        GameServices services,
        uint dirtyMask,
        Projection? previous,
        out Projection projection)
    {
        if (entity is not
            {
                Knowledge: { } knowledge,
                Position: { CellX: { } levelX, CellY: { } levelY } position
            }
            || knowledge.KnownEntity.Item is not { } item)
        {
            projection = default;
            return false;
        }

        var refreshAll = previous == null || (dirtyMask & MembershipDirty) != 0;
        var prior = previous.GetValueOrDefault();
        var refreshPosition = refreshAll || (dirtyMask & PositionDirty) != 0;
        var refreshIdentity = refreshAll || (dirtyMask & IdentityDirty) != 0;
        var refreshPerception = refreshAll || (dirtyMask & PerceptionDirty) != 0;
        var identified = knowledge.IsIdentified;
        projection = new Projection(
            position.RoomId,
            refreshPosition ? levelX : prior.LevelX,
            refreshPosition ? levelY : prior.LevelY,
            refreshIdentity ? identified ? item.Type : ItemType.None : prior.Type,
            refreshIdentity ? identified ? item.TemplateName : null : prior.BaseName,
            refreshIdentity
                ? identified ? services.Language.GetString(item, item.GetQuantity(), identified) : null
                : prior.Name,
            refreshPerception ? knowledge.SensedType.CanIdentify() : prior.IsCurrentlyPerceived);
        return true;
    }

    private static uint GetDirtyMask(IPropertyValueChanges changes)
    {
        var dirtyMask = 0u;
        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);
            if (componentId == (int)EntityComponent.Position
                && propertyName is nameof(PositionComponent.CellX) or nameof(PositionComponent.CellY))
            {
                dirtyMask |= PositionDirty;
            }
            else if (componentId == (int)EntityComponent.Knowledge)
            {
                if (propertyName == nameof(KnowledgeComponent.SensedType))
                {
                    dirtyMask |= PerceptionDirty;
                }
                else if (propertyName is nameof(KnowledgeComponent.IsIdentified)
                         or nameof(KnowledgeComponent.KnownEntityId))
                {
                    dirtyMask |= IdentityDirty;
                }
            }
            else if (componentId == (int)EntityComponent.Item)
            {
                dirtyMask |= IdentityDirty;
            }
        }

        return dirtyMask;
    }

    private static ItemChange? SerializeDelta(Projection previous, Projection current)
    {
        var change = new ItemChange();
        if (current.LevelX != previous.LevelX)
        {
            change.LevelX = current.LevelX;
        }
        if (current.LevelY != previous.LevelY)
        {
            change.LevelY = current.LevelY;
        }
        if (current.Type != previous.Type)
        {
            change.Type = current.Type;
        }
        if (current.BaseName != previous.BaseName)
        {
            change.BaseName = current.BaseName;
        }
        if (current.Name != previous.Name)
        {
            change.Name = current.Name;
        }
        if (current.IsCurrentlyPerceived != previous.IsCurrentlyPerceived)
        {
            change.IsCurrentlyPerceived = current.IsCurrentlyPerceived;
        }

         var bits = change.ChangedProperties;
         return bits.IsPropertyChanged(1)
             || bits.IsPropertyChanged(2)
             || bits.IsPropertyChanged(3)
             || bits.IsPropertyChanged(4)
             || bits.IsPropertyChanged(5)
             || bits.IsPropertyChanged(6)
            ? change
            : null;
    }

    private static ItemChange SerializeFull(Projection current)
        => new()
        {
            ChangedProperties = null,
            LevelX = current.LevelX,
            LevelY = current.LevelY,
            Type = current.Type,
            BaseName = current.BaseName,
            Name = current.Name,
            IsCurrentlyPerceived = current.IsCurrentlyPerceived
        };

    private static bool IsRelevant(GameEntity entity)
        => entity.Knowledge?.KnownEntity.Item != null;

    private static bool IsRelevantOnRemove(in EntityChange<GameEntity> entityChange)
        => entityChange.RemovedComponent is KnowledgeComponent knowledge
            && knowledge.KnownEntity.Item != null;

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
