using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Features;

public class FeatureSystem : IGameSystem<TraveledMessage>, IEntityChangeListener<GameEntity>
{
    public AbilityComponent? SelectAttack(GameEntity entity, int? tick = null)
        => SelectAttack(entity, tick ?? entity.Game.CurrentTick, includeUnavailable: false);

    private static AbilityComponent? SelectAttack(GameEntity entity, int actionTick, bool includeUnavailable)
    {
        AbilityComponent? selected = null;
        var abilities = entity.ActiveEffects?.Abilities;
        if (abilities == null || entity.Feature is { Destroyed: true }) { return null; }
        for (var index = 0; index < abilities.Count; index++)
        {
            var candidate = abilities[index];
            var ability = candidate.Ability!;
            if (!ability.Type.IsAttack() || !includeUnavailable
                && (!ability.IsUsable || ability.CooldownTick > actionTick || ability.CooldownXpLeft > 0))
            {
                continue;
            }
            if (selected == null || candidate.Id < selected.EntityId)
            {
                selected = ability;
            }
        }
        return selected;
    }

    public int GetActionDelay(GameEntity entity, AbilityComponent? attack = null)
    {
        attack ??= SelectAttack(entity) ?? SelectAttack(entity, int.MaxValue, includeUnavailable: true);
        if (attack == null)
        {
            throw new InvalidOperationException($"Timed feature '{entity.Feature!.Name}' requires an attack ability.");
        }

        var delay = entity.Manager.AbilityActivationSystem.GetDelay(attack, entity);
        var actionDelay = checked(delay + attack.Windup);
        if (delay < 0 || attack.Windup < 0 || actionDelay <= 0)
        {
            throw new InvalidOperationException($"Timed feature ability '{attack.Name}' requires a positive Delay plus Windup.");
        }
        return actionDelay;
    }

    public MessageProcessingResult Process(TraveledMessage message, GameManager manager)
    {
        if (!message.Successful || message.Entity.Being is not { IsAlive: true }
            || message.Entity.Position is not { Cell: { } cell } position
            || message.InitialLevel.Id == position.RoomId && message.InitialLevelCell == cell)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var room = position.RoomEntity.Room!;
        var threats = room.CellThreats!;
        if (!threats.TryGetValue(cell, out var candidateIds))
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var candidates = manager.IntByteListArrayPool.Rent();
        try
        {
            foreach (var entityId in candidateIds)
            {
                candidates.Add((entityId, 0));
            }
            foreach (var (entityId, _) in candidates)
            {
                if (manager.Features.FindEntity(entityId)?.Feature is not { } feature || feature.Destroyed == true)
                {
                    continue;
                }
                var definition = feature.Template;
                if (definition == null || (definition.Behavior & FeatureBehavior.TriggerOnEntry) == 0)
                {
                    continue;
                }
                var featureEntity = feature.Entity;
                var attack = SelectAttack(featureEntity, manager.Game.CurrentTick);
                if (attack != null && IsTriggerCell(featureEntity, attack, cell)
                    && (message.InitialLevel.Id != position.RoomId || !IsTriggerCell(featureEntity, attack, message.InitialLevelCell)))
                {
                    var before = feature.ThreatenedCells;
                    ActivateSelectedAttack(featureEntity, attack, message.Entity, null, manager);
                    UpdateThreats(featureEntity, before);
                }
            }
        }
        finally
        {
            candidates.Clear();
            manager.IntByteListArrayPool.Return(candidates);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public void Advance(GameEntity entity, GameManager manager)
    {
        var feature = entity.Feature!;
        var definition = feature.Template;
        if (definition?.Behavior.HasFlag(FeatureBehavior.TriggerOnInterval) != true
            || entity.Position is not { Cell: { } origin } position || feature.Destroyed == true)
        {
            SetNextActionTick(entity, null);
            return;
        }

        if (!manager.PlayerSystem.IsPlayerInRoom(position.RoomId, manager))
        {
            SetNextActionTick(entity, checked(manager.Game.CurrentTick + TimeSystem.DefaultActionDelay));
            return;
        }

        var attack = SelectAttack(entity, manager.Game.CurrentTick);
        var actionDelay = GetActionDelay(entity, attack);
        if (attack != null)
        {
            if (attack.TargetingShape is TargetingShape.Plus or TargetingShape.PlusNoCenter)
            {
                ActivateSelectedAttack(entity, attack, null, origin, manager);
            }
            else
            {
                var room = position.RoomEntity.Room!;
                for (var direction = 0; direction < 8; direction += 2)
                {
                    var vector = Vector.MovementDirections[direction];
                    var targetX = origin.X + vector.X;
                    var targetY = origin.Y + vector.Y;
                    if (targetX < 0 || targetY < 0 || targetX >= room.Width || targetY >= room.Height)
                    {
                        continue;
                    }

                    ActivateSelectedAttack(entity, attack, null, new Point((byte)targetX, (byte)targetY), manager);
                }
            }
        }

        SetNextActionTick(entity, checked(manager.Game.CurrentTick + actionDelay));
    }

    private void SetNextActionTick(GameEntity entity, int? tick)
    {
        var feature = entity.Feature!;
        var before = feature.ThreatenedCells;
        feature.NextActionTick = tick;
        UpdateThreats(entity, before);
    }

    public void UpdateThreats(GameEntity entity, Dictionary<Point, byte>? before)
    {
        var manager = entity.Manager;
        if (entity.Feature is not { Destroyed: not true, Template: { } definition } feature
            || entity.Position is not { Cell: not null })
        {
            manager.CellThreatTracker.UpdateThreats(entity, before, null);
            return;
        }
        var cells = manager.PointByteListArrayPool.Rent();
        try
        {
            if ((definition.Behavior & FeatureBehavior.TriggerOnEntry) != 0 && SelectNextEntryAttack(entity) is { } entryAttack)
            {
                GetTriggerCells(entity, cells,
                    static (target, cell) => target.Add((cell, BeveledVisibilityCalculator.MaxVisibility)), entryAttack);
            }
            if ((definition.Behavior & FeatureBehavior.TriggerOnInterval) != 0 && feature.NextActionTick is { } tick
                && SelectAttack(entity, tick) is { } attack)
            {
                GetAttackCells(entity, attack, cells,
                    static (target, cell) => target.Add((cell, BeveledVisibilityCalculator.MaxVisibility)), stopAtActors: false);
            }
            manager.CellThreatTracker.UpdateThreats(entity, before, cells);
        }
        finally
        {
            cells.Clear();
            manager.PointByteListArrayPool.Return(cells);
        }
    }

    private AbilityComponent? SelectNextEntryAttack(GameEntity entity)
    {
        if (SelectAttack(entity) is { } ready)
        {
            return ready;
        }
        var abilities = entity.ActiveEffects?.Abilities;
        if (abilities == null)
        {
            return null;
        }
        int? nextTick = null;
        for (var index = 0; index < abilities.Count; index++)
        {
            var ability = abilities[index].Ability!;
            if (ability.Type.IsAttack() && ability.IsUsable && ability.CooldownXpLeft is not > 0
                && ability.CooldownTick is { } cooldown)
            {
                nextTick = Math.Min(nextTick ?? int.MaxValue, cooldown);
            }
        }
        return nextTick is { } tick ? SelectAttack(entity, tick) : null;
    }

    public static bool IsCellThreatened(IReadOnlyDictionary<Point, HashSet<int>> cells, Point cell, int? excludedEntityId = null)
        => cells.TryGetValue(cell, out var threats)
            && (excludedEntityId == null || threats.Count > 1 || !threats.Contains(excludedEntityId.Value));

    public void GetTriggerCells<TState>(
        GameEntity entity, TState state, Action<TState, Point> add, AbilityComponent? ability = null)
    {
        ability ??= SelectAttack(entity);
        if (ability == null) { return; }
        var position = entity.Position!;
        var room = position.RoomEntity.Room!;
        var origin = position.Cell!.Value;
        if (!IsAreaTrigger(ability.TargetingShape))
        {
            var targets = (List<(Point, byte)>)entity.Manager.AbilityActivationSystem.GetTargetedCells(origin, origin,
                position.Heading ?? Direction.East, ability.Range, ability.TargetingShape, ability.TargetingShapeSize, room);
            try
            {
                foreach (var (cell, _) in targets) { add(state, cell); }
            }
            finally
            {
                targets.Clear();
                entity.Manager.PointByteListArrayPool.Return(targets);
            }
            return;
        }
        for (var targetY = Math.Max(0, origin.Y - ability.Range);
             targetY <= Math.Min(room.Height - 1, origin.Y + ability.Range); targetY++)
        {
            for (var targetX = Math.Max(0, origin.X - ability.Range);
                 targetX <= Math.Min(room.Width - 1, origin.X + ability.Range); targetX++)
            {
                var cell = new Point((byte)targetX, (byte)targetY);
                if (ContainsAreaTrigger(ability, origin, cell)) { add(state, cell); }
            }
        }
    }

    private static bool IsAreaTrigger(TargetingShape shape)
        => shape is TargetingShape.Square or TargetingShape.SquareNoCenter or TargetingShape.Plus or TargetingShape.PlusNoCenter;

    public bool IsTriggerCell(GameEntity entity, AbilityComponent ability, Point cell)
    {
        var position = entity.Position!;
        var origin = position.Cell!.Value;
        if (IsAreaTrigger(ability.TargetingShape))
        {
            return ContainsAreaTrigger(ability, origin, cell);
        }
        if (entity.Feature!.Template is { } template && (template.Behavior & FeatureBehavior.TriggerOnInterval) == 0
            && entity.Feature.ThreatenedCells is { } cached)
        {
            return cached.ContainsKey(cell);
        }
        var targets = (List<(Point, byte)>)entity.Manager.AbilityActivationSystem.GetTargetedCells(
            origin, origin, position.Heading ?? Direction.East, ability.Range, ability.TargetingShape,
            ability.TargetingShapeSize, position.RoomEntity.Room!);
        try
        {
            foreach (var (targetCell, _) in targets)
            {
                if (targetCell == cell)
                {
                    return true;
                }
            }
            return false;
        }
        finally
        {
            targets.Clear();
            entity.Manager.PointByteListArrayPool.Return(targets);
        }
    }

    private static bool ContainsAreaTrigger(AbilityComponent ability, Point origin, Point cell)
    {
        if (origin.DistanceTo(cell) > ability.Range)
        {
            return false;
        }

        return ability.TargetingShape switch
        {
            TargetingShape.Square => true,
            TargetingShape.SquareNoCenter => cell != origin,
            TargetingShape.Plus => cell.X == origin.X || cell.Y == origin.Y,
            TargetingShape.PlusNoCenter => cell != origin && (cell.X == origin.X || cell.Y == origin.Y),
            _ => false
        };
    }

    public void GetAttackCells<TState>(GameEntity entity, TState state, Action<TState, Point> add, bool stopAtActors = true)
    {
        var manager = entity.Manager;
        if (!stopAtActors && manager.CellThreatTracker.GetCachedAttackCells(entity) is { } cachedCells)
        {
            foreach (var cell in cachedCells.Keys)
            {
                add(state, cell);
            }
            return;
        }
        var ability = SelectAttack(entity, entity.Feature?.NextActionTick);
        if (ability == null) { return; }
        GetAttackCells(entity, ability, state, add, stopAtActors);
    }

    private static void GetAttackCells<TState>(GameEntity entity, AbilityComponent ability,
        TState state, Action<TState, Point> add, bool stopAtActors)
    {
        var manager = entity.Manager;
        var position = entity.Position!;
        var origin = position.Cell!.Value;
        var room = position.RoomEntity.Room!;
        if (ability.TargetingShape is TargetingShape.Plus or TargetingShape.PlusNoCenter)
        {
            var cells = (List<(Point, byte)>)manager.AbilityActivationSystem.GetTargetedCells(origin, origin,
                position.Heading!.Value, ability.Range, ability.TargetingShape, ability.TargetingShapeSize,
                room, stopAtActors);
            try
            {
                foreach (var (cell, _) in cells)
                {
                    add(state, cell);
                }
            }
            finally
            {
                cells.Clear();
                manager.PointByteListArrayPool.Return(cells);
            }
            return;
        }
        for (var direction = 0; direction < 8; direction += 2)
        {
            var vector = Vector.MovementDirections[direction];
            var targetX = origin.X + vector.X;
            var targetY = origin.Y + vector.Y;
            if (targetX < 0 || targetY < 0 || targetX >= room.Width || targetY >= room.Height)
            {
                continue;
            }

            var cells = (List<(Point, byte)>)manager.AbilityActivationSystem.GetTargetedCells(origin,
                new Point((byte)targetX, (byte)targetY), (Direction)direction, ability.Range,
                ability.TargetingShape, ability.TargetingShapeSize, room);
            try
            {
                foreach (var (cell, _) in cells)
                {
                    add(state, cell);
                    if (stopAtActors && room.Actors.ContainsKey(cell) && ability.TargetingShape == TargetingShape.Line)
                    {
                        break;
                    }
                }
            }
            finally
            {
                cells.Clear();
                manager.PointByteListArrayPool.Return(cells);
            }
        }
    }

    public void OnEntityAdded(in EntityChange<GameEntity> change)
    {
    }

    public void OnEntityRemoved(in EntityChange<GameEntity> change)
    {
    }

    public void OnPropertyValuesChanged(in EntityChange<GameEntity> change)
    {
        if (change.Entity.Feature is { Destroyed: true, NextActionTick: not null } feature)
        {
            SetNextActionTick(change.Entity, null);
        }

    }

    private static void ActivateSelectedAttack(GameEntity entity, AbilityComponent ability, GameEntity? target, Point? targetCell,
        GameManager manager)
    {
        ability.CooldownTick = null;
        ability.CooldownXpLeft = null;
        var activate = ActivateAbilityMessage.Create(manager);
        activate.AbilityEntity = ability.Entity;
        activate.ActivatorEntity = entity;
        activate.TargetEntity = target;
        activate.TargetCell = targetCell;
        manager.Process(activate);
    }
}
