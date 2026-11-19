using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Features;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public sealed class FeatureChangeBuilder : IDependentEntityChangeListener<GameEntity>, IEntityChangeListener<GameEntity>
{
    private readonly record struct Projection(int RoomId, string Name, int X, int Y, bool Destroyed, bool BlocksMovement,
        int? NextActionTick, int? CooldownTick, string? Attack, int Range, TargetingShape Shape, int ShapeSize, int[] Cells,
        bool IsCurrentlyPerceived);

    private readonly HashSet<int> _pending = [];
    private readonly Dictionary<int, Projection> _projection = [];
    private readonly Dictionary<int, Projection> _prepared = [];
    private readonly HashSet<int> _geometryDirty = [];

    public void RegisterOnGroups(GameManager manager)
    {
        manager.KnownPositions.AddListener(this);
        manager.KnownFeaturesToCellRelationship.AddDependentsListener(this);
        manager.FeaturesToRoomCellRelationship.AddDependentsListener(this);
        manager.AbilitiesToAffectableRelationship.AddDependentsListener(this);
        manager.RoomActorsToRoomCellRelationship.AddDependentsListener(this);
    }

    public void UnregisterFromGroups(GameManager manager)
    {
        manager.KnownPositions.RemoveListener(this);
        manager.KnownFeaturesToCellRelationship.RemoveDependentsListener(this);
        manager.FeaturesToRoomCellRelationship.RemoveDependentsListener(this);
        manager.AbilitiesToAffectableRelationship.RemoveDependentsListener(this);
        manager.RoomActorsToRoomCellRelationship.RemoveDependentsListener(this);
    }

    public void Reset()
    {
        _pending.Clear();
        _prepared.Clear();
        _geometryDirty.Clear();
    }

    public void ResetProjection(GameEntity player)
    {
        _projection.Clear();
        _prepared.Clear();
        _geometryDirty.Clear();
        if (player.Position?.RoomEntity.Room is not { } room)
        {
            return;
        }

        foreach (var knowledge in room.KnownFeatures.Values)
        {
            var feature = knowledge.Knowledge!.KnownEntity;
            _projection[feature.Id] = Read(feature);
        }
    }

    public bool HasChangesForRoom(RoomComponent room)
    {
        if (room.TerrainChanges is { Count: > 0 })
        {
            MarkGeometry(room, includeEntry: true);
        }

        foreach (var id in _pending)
        {
            var entity = room.Entity.Manager.FindEntity(id);
            if (HasChange(id, entity, room.EntityId))
            {
                return true;
            }
        }

        return false;
    }

    public Dictionary<int, FeatureChange>? BuildChanges(RoomComponent room)
    {
        if (room.TerrainChanges is { Count: > 0 })
        {
            MarkGeometry(room, includeEntry: true);
        }
        Dictionary<int, FeatureChange>? output = null;
        foreach (var id in _pending)
        {
            var entity = room.Entity.Manager.FindEntity(id);
            if (!HasChange(id, entity, room.EntityId))
            {
                continue;
            }

            output ??= [];
            if (!IsKnown(entity, room.EntityId))
            {
                output[id] = new FeatureChange { ChangedProperties = 0 };
                _projection.Remove(id);
                continue;
            }

            var current = ReadCurrent(entity!);
            output[id] = _projection.TryGetValue(id, out var previous) && previous.RoomId == room.EntityId
                ? Delta(previous, current, room)
                : Snapshot(current, room);
            _projection[id] = current;
        }

        return output;
    }

    public static FeatureChange SerializeFeature(GameEntity entity) => Snapshot(Read(entity), entity.Position!.RoomEntity.Room!);

    private static bool IsKnown(GameEntity? entity, int roomId)
        => entity?.Feature != null && entity.Position?.Knowledge?.Position?.RoomId == roomId;

    private bool HasChange(int id, GameEntity? entity, int roomId)
    {
        var known = _projection.TryGetValue(id, out var previous) && previous.RoomId == roomId;
        if (!IsKnown(entity, roomId))
        {
            return known;
        }

        var current = ReadCurrent(entity!);
        return !known || previous with
        {
            Cells = current.Cells
        } != current
            || !previous.Cells.AsSpan().SequenceEqual(current.Cells);
    }

    private Projection ReadCurrent(GameEntity entity)
    {
        if (!_prepared.TryGetValue(entity.Id, out var current))
        {
            current = Read(entity, !_geometryDirty.Contains(entity.Id) && _projection.TryGetValue(entity.Id, out var previous)
                ? previous : null);
            _prepared[entity.Id] = current;
        }
        return current;
    }

    private static Projection Read(GameEntity entity, Projection? previous = null)
    {
        var feature = entity.Feature!;
        var knowledgeEntity = entity.Position?.Knowledge
            ?? throw new InvalidOperationException($"Feature {entity.Id} has not been perceived.");
        var knowledge = knowledgeEntity.Knowledge!;
        var position = knowledgeEntity.Position!;
        var name = knowledge.IsIdentified ? feature.Name : "";
        var definition = knowledge.IsIdentified ? feature.Template : null;
        var currentAttack = knowledge.IsIdentified ? entity.Manager.FeatureSystem.SelectAttack(entity) : null;
        var ability = definition?.Behavior.HasFlag(FeatureBehavior.TriggerOnInterval) == true
            ? entity.Manager.FeatureSystem.SelectAttack(entity, feature.NextActionTick) : currentAttack;
        var cooldownTick = knowledge.IsIdentified
            ? currentAttack != null ? currentAttack.CooldownTick
                : entity.Manager.FeatureSystem.SelectAttack(entity, int.MaxValue)?.CooldownTick
            : null;
        var cells = previous is { } known && known.RoomId == position.RoomId && known.Name == name
            && known.X == position.CellX && known.Y == position.CellY && known.Destroyed == (feature.Destroyed == true)
            && known.Attack == ability?.Name && known.Range == (ability?.Range ?? 0)
            && known.Shape == (ability?.TargetingShape ?? default) && known.ShapeSize == (ability?.TargetingShapeSize ?? 0)
                ? known.Cells
                : GetAffectedCells(entity, definition?.Behavior ?? FeatureBehavior.None);
        return new Projection(position.RoomId, name, position.CellX!.Value, position.CellY!.Value,
            feature.Destroyed == true, feature.BlocksMovement, feature.NextActionTick, cooldownTick,
            ability?.Name, ability?.Range ?? 0, ability?.TargetingShape ?? default, ability?.TargetingShapeSize ?? 0,
            cells, knowledge.SensedType.CanIdentify());
    }

    private static int[] GetAffectedCells(GameEntity entity, FeatureBehavior behavior)
    {
        if (entity.Feature!.Destroyed == true) { return []; }
        var system = entity.Manager.FeatureSystem;
        var cells = new HashSet<Point>();
        if (behavior.HasFlag(FeatureBehavior.TriggerOnEntry))
        {
            system.GetTriggerCells(entity, cells, static (target, cell) => target.Add(cell));
        }
        if (behavior.HasFlag(FeatureBehavior.TriggerOnInterval))
        {
            system.GetAttackCells(entity, cells, static (target, cell) => target.Add(cell));
        }
        return cells.Select(cell => cell.ToInt32()).ToArray();
    }

    private static FeatureChange Snapshot(Projection value, RoomComponent room) => new()
    {
        ChangedProperties = null,
        Name = value.Name,
        LevelX = value.X,
        LevelY = value.Y,
        Destroyed = value.Destroyed,
        BlocksMovement = value.BlocksMovement,
        NextActionTick = value.NextActionTick,
        CooldownTick = value.CooldownTick,
        NextActions = Actions(value, room),
        AffectedCells = value.Cells,
        IsCurrentlyPerceived = value.IsCurrentlyPerceived
    };

    private static FeatureChange Delta(Projection previous, Projection current, RoomComponent room)
    {
        var result = new FeatureChange();
        if (previous.Name != current.Name)
        {
            result.Name = current.Name;
        }
        if (previous.X != current.X)
        {
            result.LevelX = current.X;
        }
        if (previous.Y != current.Y)
        {
            result.LevelY = current.Y;
        }
        if (previous.Destroyed != current.Destroyed)
        {
            result.Destroyed = current.Destroyed;
        }
        if (previous.BlocksMovement != current.BlocksMovement)
        {
            result.BlocksMovement = current.BlocksMovement;
        }
        if (previous.NextActionTick != current.NextActionTick)
        {
            result.NextActionTick = current.NextActionTick;
        }
        if (previous.CooldownTick != current.CooldownTick)
        {
            result.CooldownTick = current.CooldownTick;
        }
        if ((previous.NextActionTick.HasValue || current.NextActionTick.HasValue)
            && (previous.Attack != current.Attack || previous.Range != current.Range || previous.Shape != current.Shape
            || previous.ShapeSize != current.ShapeSize || previous.X != current.X || previous.Y != current.Y
            || previous.Destroyed != current.Destroyed
            || previous.NextActionTick.HasValue != current.NextActionTick.HasValue))
        {
            result.NextActions = Actions(current, room);
        }
        if (!previous.Cells.AsSpan().SequenceEqual(current.Cells))
        {
            result.AffectedCells = current.Cells;
        }
        if (previous.IsCurrentlyPerceived != current.IsCurrentlyPerceived)
        {
            result.IsCurrentlyPerceived = current.IsCurrentlyPerceived;
        }
        return result;
    }

    private static ActorActionChange[]? Actions(Projection value, RoomComponent room)
    {
        if (value.NextActionTick == null || value.Attack == null || value.Destroyed)
        {
            return null;
        }

        var cross = value.Shape is TargetingShape.Plus or TargetingShape.PlusNoCenter;
        var result = new ActorActionChange[cross ? 1 : 4];
        for (var index = 0; index < result.Length; index++)
        {
            var vector = Vector.MovementDirections[index * 2];
            var target = cross ? new Point((byte)value.X, (byte)value.Y)
                : new Point((byte)Math.Clamp(value.X + vector.X * value.Range, 0, room.Width - 1),
                    (byte)Math.Clamp(value.Y + vector.Y * value.Range, 0, room.Height - 1));
            result[index] = new ActorActionChange
            {
                Type = ActorActionType.UseAbilitySlot,
                Name = value.Attack,
                Target = target.ToInt32(),
                TargetingShape = value.Shape,
                TargetingShapeSize = value.ShapeSize
            };
        }

        return result;
    }

    private void MarkGeometry(RoomComponent room, bool includeEntry = false)
    {
        foreach (var knowledge in room.KnownFeatures.Values)
        {
            var feature = knowledge.Knowledge!.KnownEntity;
            if (feature.Feature!.NextActionTick != null || includeEntry
                && feature.Game.Services.Data.Features.Find(feature.Feature.Name)?.Behavior.HasFlag(FeatureBehavior.TriggerOnEntry) == true)
            {
                _pending.Add(feature.Id);
                _prepared.Remove(feature.Id);
                _geometryDirty.Add(feature.Id);
            }
        }
    }

    private void Track(in EntityChange<GameEntity> change, GameEntity principal)
    {
        var knowledge = change.Entity.Knowledge
            ?? (change.RemovedComponent is KnowledgeComponent removed ? removed : null);
        if (knowledge != null && (knowledge.KnownEntity.Feature != null || _projection.ContainsKey(knowledge.KnownEntityId)))
        {
            _pending.Add(knowledge.KnownEntityId);
            _prepared.Remove(knowledge.KnownEntityId);
            _geometryDirty.Add(knowledge.KnownEntityId);
            return;
        }

        if (principal.Feature != null)
        {
            _pending.Add(principal.Id);
            _prepared.Remove(principal.Id);
            _geometryDirty.Add(principal.Id);
        }
        else if (change.Entity.Feature != null || _projection.ContainsKey(change.Entity.Id))
        {
            _pending.Add(change.Entity.Id);
            _prepared.Remove(change.Entity.Id);
            for (var index = 0; index < change.PropertyChanges.Count; index++)
            {
                if (change.PropertyChanges.GetChangedComponent(index).ComponentId == (int)EntityComponent.Position)
                {
                    _geometryDirty.Add(change.Entity.Id);
                    break;
                }
            }
        }
        else if (principal.Room is { } room && change.Entity.Being != null)
        {
            MarkGeometry(room);
        }
    }

    public void OnEntityAdded(in EntityChange<GameEntity> change, GameEntity principal) => Track(change, principal);
    public void OnEntityRemoved(in EntityChange<GameEntity> change, GameEntity principal) => Track(change, principal);
    public void OnPropertyValuesChanged(in EntityChange<GameEntity> change, GameEntity principal) => Track(change, principal);

    public void OnEntityAdded(in EntityChange<GameEntity> change)
    {
    }

    public void OnEntityRemoved(in EntityChange<GameEntity> change)
    {
    }

    public void OnPropertyValuesChanged(in EntityChange<GameEntity> change) => Track(change, change.Entity);
}
