using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Features;

public sealed class CellThreatTracker : IEntityChangeListener<GameEntity>, IDependentEntityChangeListener<GameEntity>
{
    private const uint ThreatSourceMask = (1u << (int)EntityComponent.Feature) | (1u << (int)EntityComponent.AI);
    private readonly GameManager _manager;
    private readonly HashSet<int> _dirtySources = [];
    private readonly Dictionary<Point, byte> _nextCells = [];
    private bool _updating;

    public CellThreatTracker(GameManager manager)
    {
        _manager = manager;
        manager.Features.AddListener(this);
    }

    public void UpdateThreats(GameEntity source, Dictionary<Point, byte>? before,
        IReadOnlyList<(Point Cell, byte Exposure)>? after)
    {
        var previousUpdating = _updating;
        _updating = true;
        try
        {
            _nextCells.Clear();
            if (after != null)
            {
                for (var index = 0; index < after.Count; index++)
                {
                    var (cell, exposure) = after[index];
                    _nextCells[cell] = Math.Max(_nextCells.GetValueOrDefault(cell), exposure);
                }
            }
            if (source.Position?.RoomEntity.Room?.CellThreats is { } cells)
            {
                UpdateCells(cells, source.Id, before, _nextCells);
            }
            var current = before;
            if (_nextCells.Count == 0)
            {
                current?.Clear();
                current = null;
            }
            else
            {
                current ??= [];
                current.Clear();
                foreach (var (cell, exposure) in _nextCells)
                {
                    current.Add(cell, exposure);
                }
            }
            if (source.Feature is { } feature)
            {
                feature.ThreatenedCells = current;
            }
            else if (source.AI is { } ai)
            {
                ai.ThreatenedCells = current;
            }
            _dirtySources.Remove(source.Id);
        }
        finally
        {
            _updating = previousUpdating;
        }
    }

    public Dictionary<Point, HashSet<int>>? GetThreats(RoomComponent room, Dictionary<Point, HashSet<int>>? cells)
    {
        if (_updating)
        {
            return cells;
        }
        _updating = true;
        try
        {
            if (cells == null)
            {
                cells = [];
                room.CellThreats = cells;
                ResetRoomSources(room);
            }
            while (_dirtySources.Count != 0)
            {
                using var enumerator = _dirtySources.GetEnumerator();
                enumerator.MoveNext();
                var sourceId = enumerator.Current;
                _dirtySources.Remove(sourceId);
                UpdateSource(sourceId);
            }
            return cells;
        }
        finally
        {
            _updating = false;
        }
    }

    public void TerrainChanged(RoomComponent room)
    {
        foreach (var source in room.Features.Values)
        {
            _dirtySources.Add(source.Id);
        }
        foreach (var source in room.Actors.Values)
        {
            if (source.HasComponent(EntityComponent.AI))
            {
                ClearThreats(source);
            }
        }
    }

    public void ClearThreats(GameEntity source)
    {
        RemoveCachedSource(new EntityChange<GameEntity>(source));
        if (source.AI is { } ai)
        {
            ai.ThreatenedCells = null;
        }
        else if (source.Feature is { } feature)
        {
            feature.ThreatenedCells = null;
        }
        _dirtySources.Add(source.Id);
    }

    public Dictionary<Point, byte>? GetCachedTargetCells(GameEntity source, int abilityId, Point targetCell)
    {
        if (source.AI is not { ThreatenedCells: { } cells } ai || source.Position?.Cell == null
            || ai.NextAction is not (null or ActorActionType.UseAbilitySlot))
        {
            return null;
        }
        if (ai.NextAction == ActorActionType.UseAbilitySlot
            && (_dirtySources.Contains(source.Id)
                || ai.NextActionTarget is not { } slot || _manager.AbilitySlottingSystem.GetAbility(source, slot)?.Id != abilityId
                || ActorSystemBase.ResolveAbilityTargetCell(source, ai.NextActionTarget2, _manager) != targetCell))
        {
            return null;
        }
        return cells;
    }

    public Dictionary<Point, byte>? GetCachedAttackCells(GameEntity source)
    {
        if (_updating || source.Feature is not { NextActionTick: not null, Template: { } template } feature
            || (template.Behavior & FeatureBehavior.TriggerOnEntry) != 0)
        {
            return null;
        }
        _ = source.Position!.RoomEntity.Room!.CellThreats;
        return feature.ThreatenedCells;
    }

    public void OnEntityAdded(in EntityChange<GameEntity> change)
    {
        if (change.Entity.HasAnyComponent(ThreatSourceMask))
        {
            _dirtySources.Add(change.Entity.Id);
        }
    }

    public void OnEntityRemoved(in EntityChange<GameEntity> change)
        => RemoveCachedSource(change);

    public void OnPropertyValuesChanged(in EntityChange<GameEntity> change)
    {
        if (change.RemovedComponent is AIComponent or FeatureComponent)
        {
            RemoveCachedSource(change);
        }
        if (!change.Entity.HasAnyComponent(ThreatSourceMask))
        {
            return;
        }
        var properties = change.PropertyChanges;
        for (var index = 0; index < properties.Count; index++)
        {
            if (properties.GetChangedComponent(index).ComponentId == (int)EntityComponent.Position
                && (change.Entity.AI != null || properties.GetChangedPropertyName(index) == nameof(PositionComponent.RoomId)))
            {
                RemoveCachedSource(change);
                if (change.Entity.AI is { } ai)
                {
                    ai.ThreatenedCells = null;
                }
                else if (change.Entity.Feature is { } feature)
                {
                    feature.ThreatenedCells = null;
                }
                break;
            }
        }
        if (change.Entity.AI != null || change.RemovedComponent is AIComponent
            || change.AddedComponent != null || change.RemovedComponent != null)
        {
            _dirtySources.Add(change.Entity.Id);
            return;
        }
        for (var index = 0; index < properties.Count; index++)
        {
            if (properties.GetChangedComponent(index).ComponentId == (int)EntityComponent.Position
                || properties.GetChangedComponent(index).ComponentId == (int)EntityComponent.Feature
                && properties.GetChangedPropertyName(index) is nameof(FeatureComponent.Name)
                    or nameof(FeatureComponent.Destroyed) or nameof(FeatureComponent.NextActionTick))
            {
                _dirtySources.Add(change.Entity.Id);
                return;
            }
        }
    }

    public void OnEntityAdded(in EntityChange<GameEntity> change, GameEntity principal) => DependencyChanged(change, principal);
    public void OnEntityRemoved(in EntityChange<GameEntity> change, GameEntity principal) => DependencyChanged(change, principal);
    public void OnPropertyValuesChanged(in EntityChange<GameEntity> change, GameEntity principal)
    {
        var properties = change.PropertyChanges;
        if (properties.Count == 1 && properties.GetChangedComponent(0) is AbilityComponent { CooldownTick: null }
            && properties.GetChangedPropertyName(0) == nameof(AbilityComponent.CooldownTick))
        {
            return;
        }
        DependencyChanged(change, principal);
    }

    private void DependencyChanged(in EntityChange<GameEntity> change, GameEntity principal)
    {
        if (principal.HasAnyComponent(ThreatSourceMask) && principal.Position != null)
        {
            if (principal.AI != null)
            {
                ClearThreats(principal);
            }
            _dirtySources.Add(principal.Id);
            return;
        }
        if (principal.Room is not { } room)
        {
            return;
        }
        if (change.Entity.Feature != null || change.Entity.AI != null || change.RemovedComponent is AIComponent)
        {
            _dirtySources.Add(change.Entity.Id);
        }
        foreach (var source in room.Actors.Values)
        {
            if (source.AI is { NextAction: ActorActionType.UseAbilitySlot, NextActionTarget2: < 0 } ai
                && ai.NextActionTarget2 == -change.Entity.Id)
            {
                ClearThreats(source);
            }
        }
    }

    private void UpdateSource(int sourceId)
    {
        var source = _manager.FindEntity(sourceId);
        if (source?.Position?.RoomEntity.Room == null || !source.HasAnyComponent(ThreatSourceMask))
        {
            return;
        }
        if (source.Feature is { } feature)
        {
            _manager.FeatureSystem.UpdateThreats(source, feature.ThreatenedCells);
        }
        else if (source.AI is { } ai)
        {
            _manager.AISystem.UpdateThreats(source, ai.ThreatenedCells);
        }
    }

    private static void UpdateCells(Dictionary<Point, HashSet<int>> cells, int sourceId,
        Dictionary<Point, byte>? previous, Dictionary<Point, byte> current)
    {
        if (previous != null)
        {
            foreach (var cell in previous.Keys)
            {
                if (!current.ContainsKey(cell))
                {
                    RemoveSource(cells, cell, sourceId);
                }
            }
        }
        foreach (var cell in current.Keys)
        {
            if (previous?.ContainsKey(cell) == true)
            {
                continue;
            }
            if (!cells.TryGetValue(cell, out var sources))
            {
                sources = [];
                cells.Add(cell, sources);
            }
            sources.Add(sourceId);
        }
    }

    private static void RemoveCachedCells(Dictionary<Point, HashSet<int>> cells, Dictionary<Point, byte> cached, int sourceId)
    {
        foreach (var cell in cached.Keys)
        {
            RemoveSource(cells, cell, sourceId);
        }
    }

    private static void RemoveSource(Dictionary<Point, HashSet<int>> cells, Point cell, int sourceId)
    {
        if (cells.TryGetValue(cell, out var sources) && sources.Remove(sourceId) && sources.Count == 0)
        {
            cells.Remove(cell);
        }
    }

    private void RemoveCachedSource(in EntityChange<GameEntity> change)
    {
        var entity = change.Entity;
        var feature = change.RemovedComponent as FeatureComponent ?? entity.Feature;
        var ai = feature == null ? change.RemovedComponent as AIComponent ?? entity.AI : null;
        var cached = feature?.ThreatenedCells ?? ai?.ThreatenedCells;
        var position = change.RemovedComponent as PositionComponent ?? entity.Position;
        var room = position?.RoomEntity.Room;
        var properties = change.PropertyChanges;
        for (var index = 0; index < properties.Count; index++)
        {
            if (properties.GetChangedComponent(index).ComponentId == (int)EntityComponent.Position
                && properties.GetChangedPropertyName(index) == nameof(PositionComponent.RoomId))
            {
                room = _manager.FindEntity(properties.GetValue<int>(index, Utils.MessagingECS.ValueType.Old))?.Room;
                break;
            }
        }
        var previousUpdating = _updating;
        _updating = true;
        try
        {
            if (cached != null && room?.CellThreats is { } cells)
            {
                RemoveCachedCells(cells, cached, entity.Id);
            }
            cached?.Clear();
            _dirtySources.Remove(entity.Id);
        }
        finally
        {
            _updating = previousUpdating;
        }
    }

    private void ResetRoomSources(RoomComponent room)
    {
        foreach (var source in room.Features.Values)
        {
            source.Feature!.ThreatenedCells?.Clear();
            _dirtySources.Add(source.Id);
        }
        foreach (var source in room.Actors.Values)
        {
            if (source.HasComponent(EntityComponent.AI))
            {
                source.AI!.ThreatenedCells?.Clear();
                _dirtySources.Add(source.Id);
            }
        }
    }
}