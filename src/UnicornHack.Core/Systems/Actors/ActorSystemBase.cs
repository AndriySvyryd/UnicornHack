using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;

namespace UnicornHack.Systems.Actors;

public abstract class ActorSystemBase
{
    private static readonly ActorActionType[] ConfusableActions =
    [
        ActorActionType.ChangeHeading,
        ActorActionType.MoveOneCell,
        ActorActionType.MoveToCell,
        ActorActionType.PickupItem,
        ActorActionType.DropItem,
        ActorActionType.EquipItem,
        ActorActionType.UnequipItem,
        ActorActionType.UseAbilitySlot,
        ActorActionType.TravelToRoom
    ];

    private static readonly int MovementNoise = 50;
    private static readonly int AttackNoise = 100;
    private static readonly int VocalNoise = 100;

    protected static void RandomizeActionTargetIfConfused(
        GameEntity actorEntity, ref ActorActionType action, ref int? target, ref int? target2,
        GameManager manager)
    {
        if (action == ActorActionType.SetAbilitySlot)
        {
            return;
        }

        var randomizePercent = action is ActorActionType.ChangeHeading
            or ActorActionType.MoveOneCell
            or ActorActionType.MoveToCell
            ? actorEntity.Being!.RandomMovementPercent
            : actorEntity.Being!.RandomActionPercent;
        if (randomizePercent == 0
            || randomizePercent < 100
            && manager.Game.Random.Next(0, 100) >= randomizePercent)
        {
            return;
        }

        var originalTarget = target;
        var originalTarget2 = target2;
        if (TryPickActionTarget(
                actorEntity, action, originalTarget, originalTarget2,
                excludeOriginal: true, ref target, ref target2, manager))
        {
            return;
        }

        var remainingActions = new List<ActorActionType>(ConfusableActions);
        remainingActions.Remove(action);
        while (remainingActions.Count != 0)
        {
            var actionIndex = manager.Game.Random.Next(0, remainingActions.Count);
            var candidateAction = remainingActions[actionIndex];
            remainingActions.RemoveAt(actionIndex);
            int? candidateTarget = null;
            int? candidateTarget2 = null;
            if (!TryPickActionTarget(
                    actorEntity, candidateAction, originalTarget: null, originalTarget2: null,
                    excludeOriginal: false, ref candidateTarget, ref candidateTarget2, manager))
            {
                continue;
            }

            action = candidateAction;
            target = candidateTarget;
            target2 = candidateTarget2;
            return;
        }

        action = ActorActionType.Wait;
        target = null;
        target2 = null;
    }

    private static bool TryPickActionTarget(
        GameEntity actorEntity, ActorActionType action, int? originalTarget, int? originalTarget2,
        bool excludeOriginal, ref int? target, ref int? target2, GameManager manager)
    {
        var candidates = new List<(int Target, int? Target2)>();
        var position = actorEntity.Position!;
        var being = actorEntity.Being!;

        switch (action)
        {
            case ActorActionType.ChangeHeading:
                for (var direction = 0; direction < 8; direction++)
                {
                    if (direction != (int?)position.Heading)
                    {
                        AddCandidate(direction, null);
                    }
                }

                break;
            case ActorActionType.MoveOneCell:
            case ActorActionType.MoveToCell:
            {
                if (being.CanMove
                    && (action != ActorActionType.MoveToCell || actorEntity.Player != null))
                {
                    Span<Direction> directions = stackalloc Direction[8];
                    var directionCount = manager.TravelSystem.GetPossibleMovementDirections(
                        position, safe: true, directions);
                    for (var i = 0; i < directionCount; i++)
                    {
                        var direction = directions[i];
                        AddCandidate(action == ActorActionType.MoveOneCell
                            ? (int)direction
                            : position.Cell!.Value.Translate(direction.AsVector()).ToInt32(), null);
                    }
                }

                break;
            }
            case ActorActionType.PickupItem:
                if (actorEntity.Player != null
                    && being.CanMoveItems
                    && position.RoomEntity.Room!.Items.ContainsKey(position.Cell!.Value))
                {
                    for (var slot = AbilitySlottingSystem.DefaultRangedAttackSlot + 1;
                         slot < actorEntity.Physical!.Capacity;
                         slot++)
                    {
                        if (!being.SlottedAbilities.ContainsKey(slot))
                        {
                            AddCandidate(slot, null);
                        }
                    }
                }

                break;
            case ActorActionType.DropItem:
                if (being.CanMoveItems)
                {
                    foreach (var item in being.Items)
                    {
                        AddCandidate(item.Id, null);
                    }
                }

                break;
            case ActorActionType.EquipItem:
                if (being.CanMoveItems)
                {
                    foreach (var item in being.Items)
                    {
                        foreach (var slot in manager.ItemUsageSystem.GetEquipableSlots(item.Item!, actorEntity))
                        {
                            var equipMessage = EquipItemMessage.Create(manager);
                            equipMessage.ActorEntity = actorEntity;
                            equipMessage.ItemEntity = item;
                            equipMessage.Slot = slot;
                            equipMessage.Force = actorEntity.Player != null;
                            var canEquip = manager.ItemUsageSystem.CanEquipItem(equipMessage, manager);
                            manager.ReturnMessage(equipMessage);
                            if (canEquip)
                            {
                                AddCandidate(item.Id, (int)slot);
                            }
                        }
                    }
                }

                break;
            case ActorActionType.UnequipItem:
                if (being.CanMoveItems)
                {
                    foreach (var item in being.Items)
                    {
                        if (item.Item!.EquippedSlot != EquipmentSlot.None)
                        {
                            AddCandidate(item.Id, null);
                        }
                    }
                }

                break;
            case ActorActionType.UseAbilitySlot:
                if (originalTarget is { } abilitySlot)
                {
                    AddAbilityTargets(abilitySlot);
                }
                else
                {
                    foreach (var slottedAbility in being.SlottedAbilities.Keys)
                    {
                        AddAbilityTargets(slottedAbility);
                    }
                }

                break;
            case ActorActionType.TravelToRoom:
                if (actorEntity.Player != null
                    && being.CanMove
                    && EffectApplicationSystem.GetAbilityLevel(actorEntity, AbilityData.Engulfed.Name) == 0
                    && manager.EngulferToVictimRelationship.GetVictim(actorEntity) == null)
                {
                    var room = position.RoomEntity.Room!;
                    var hasHostiles = room.Actors.Values.Any(actor => actor.HasComponent(EntityComponent.AI));
                    foreach (var connectionEntity in room.Connections.Values)
                    {
                        if (!hasHostiles || position.Cell == connectionEntity.Position!.Cell)
                        {
                            AddCandidate(connectionEntity.Connection!.TargetRoomId, null);
                        }
                    }
                }

                break;
        }

        if (candidates.Count == 0)
        {
            return false;
        }

        var selected = candidates.Count == 1
            ? candidates[0]
            : candidates[manager.Game.Random.Next(0, candidates.Count)];
        target = selected.Target;
        target2 = selected.Target2;
        return true;

        void AddCandidate(int candidateTarget, int? candidateTarget2)
        {
            if (!excludeOriginal
                || candidateTarget != originalTarget
                || candidateTarget2 != originalTarget2)
            {
                candidates.Add((candidateTarget, candidateTarget2));
            }
        }

        void AddAbilityTargets(int abilitySlot)
        {
            if (manager.AbilitySlottingSystem.GetAbility(actorEntity, abilitySlot) is not { } abilityEntity
                || (abilityEntity.Ability!.Activation & ActivationType.Targeted) == 0)
            {
                return;
            }

            foreach (var legalTarget in GetLegalAbilityTargets(actorEntity, abilityEntity, manager))
            {
                AddCandidate(abilitySlot, legalTarget);
            }
        }
    }

    private static List<int> GetLegalAbilityTargets(
        GameEntity actorEntity, GameEntity abilityEntity, GameManager manager)
    {
        var room = actorEntity.Position!.RoomEntity.Room!;
        var legalTargets = new List<int>();
        foreach (var cell in room.IndexToPoint!)
        {
            var targetActor = room.Actors.GetValueOrDefault(cell);
            var message = ActivateAbilityMessage.Create(manager);
            message.AbilityEntity = abilityEntity;
            message.ActivatorEntity = actorEntity;
            message.TargetEntity = targetActor;
            message.TargetCell = cell;
            var error = manager.AbilityActivationSystem.CanActivateAbility(message);
            manager.ReturnMessage(message);
            if (error == null)
            {
                legalTargets.Add(targetActor?.Being is { IsAlive: true }
                    ? -targetActor.Id
                    : cell.ToInt32());
            }
        }

        return legalTargets;
    }

    protected static void EnqueueActionNoise(
        GameEntity actorEntity, ActorActionType action, int? target, GameManager manager)
    {
        SoundMessage.Enqueue(actorEntity, GetActionNoise(actorEntity, action, target, manager), manager);
    }

    public static int GetActionNoise(
        GameEntity actorEntity, ActorActionType action, int? target, GameManager manager)
    {
        if (action is ActorActionType.MoveOneCell or ActorActionType.MoveToCell)
        {
            return MovementNoise;
        }

        if (action != ActorActionType.UseAbilitySlot
            || target is not { } slot
            || manager.AbilitySlottingSystem.GetAbility(actorEntity, slot)?.Ability is not { } ability)
        {
            return 0;
        }

        return GetAbilityNoise(ability);
    }

    public static int GetAbilityNoise(AbilityComponent ability)
        => ability.Noise
           ?? (AbilityActivationSystem.IsAttack(ability)
               ? AttackNoise
               : ability.Action is AbilityAction.Spell
                   or AbilityAction.Explosion
                   or AbilityAction.Scream
                   or AbilityAction.Screech
                   ? VocalNoise
                   : 0);

    protected static bool IsAllowedWhileTaunted(GameEntity actorEntity, ActorActionType action)
        => EffectApplicationSystem.GetAbilityLevel(actorEntity, AbilityData.Taunted.Name) < 3
            || action is ActorActionType.ChangeHeading
                or ActorActionType.MoveOneCell
                or ActorActionType.MoveToCell
                or ActorActionType.UseAbilitySlot;

    /// <summary>
    ///     Realizes at the start of the actor's turn everything that happened to it since it last acted,
    ///     scaled by the elapsed time - lingering damage and the threshold build-ups (fading, detonating a
    ///     doom/overcharge counter, or forcing a lycanthropy transformation) - then records the current tick.
    /// </summary>
    protected static void ProcessTimedEffects(GameEntity actorEntity, ActorComponent actor, GameManager manager)
    {
        var effectSystem = manager.EffectApplicationSystem;
        var currentTick = manager.Game.CurrentTick;
        var previousTick = actor.PreviousActionTick ?? currentTick;
        var elapsed = currentTick - previousTick;

        // Lingering resource changes only accrue over elapsed time, but build-ups must still be realized when no
        // time has passed (e.g. applied just before the actor's first turn).
        if (elapsed > 0
            && actorEntity.ActiveEffects is { } activeEffects)
        {
            effectSystem.ApplyLingeringResourceEffects(actorEntity, elapsed);
            ActiveEffectsComponent.ForEachEffect(
                activeEffects.PropertyEffects,
                (EffectSystem: effectSystem, ActorEntity: actorEntity, Elapsed: elapsed, Manager: manager),
                static (activeEffect, state) =>
                {
                    if (activeEffect.CombinationFunction.IsBuildUp())
                    {
                        state.EffectSystem.ApplyLingeringBuildUpEffect(
                            activeEffect, state.ActorEntity, state.Elapsed, state.Manager);
                    }
                });
        }

        effectSystem.RealizeBuildUps(actorEntity, elapsed, manager);

        actor.PreviousActionTick = currentTick;
    }

    protected static bool CanAct(GameEntity actorEntity, GameManager manager)
    {
        var unableToActPercent = actorEntity.Being!.UnableToActPercent;
        return unableToActPercent < 100
            && (unableToActPercent == 0
                || manager.Game.Random.Next(0, 100) >= unableToActPercent);
    }

    protected bool ActivateAbility(GameEntity actorEntity, int? slot, int? targetId, GameManager manager)
    {
        Point targetCell;
        GameEntity? targetActor;
        if (targetId == null)
        {
            targetCell = actorEntity.Position!.Cell!.Value;
            targetActor = actorEntity;
        }
        else if (targetId.Value < 0)
        {
            targetActor = manager.FindEntity(-targetId.Value);
            if (targetActor?.Being is not { IsAlive: true }
                || targetActor.Position is not { Cell: { } targetActorCell })
            {
                return false;
            }

            targetCell = targetActorCell;
        }
        else
        {
            targetCell = Point.Unpack(targetId)!.Value;
            var room = actorEntity.Position!.RoomEntity.Room!;
            if (!room.IsValid(targetCell))
            {
                if (actorEntity.HasComponent(EntityComponent.Player))
                {
                    manager.LoggingSystem.WriteLog(
                        manager.Game.Services.Language.NoPath(), actorEntity, manager);
                    return false;
                }

                throw new InvalidOperationException(
                    "Actor " + actorEntity.Id + ". Target cell " + targetCell + " is outside the room.");
            }

            targetActor = room.Actors.GetValueOrDefault(targetCell);
        }

        GameEntity? abilityEntity;
        if (slot.HasValue)
        {
            abilityEntity = manager.AbilitySlottingSystem.GetAbility(actorEntity, slot.Value);
            if (abilityEntity == null)
            {
                if (actorEntity.HasComponent(EntityComponent.Player))
                {
                    manager.LoggingSystem.WriteLog(
                        manager.Game.Services.Language.NoAbilityInSlot(slot.Value), actorEntity, manager);
                }
                else
                {
                    throw new InvalidOperationException(
                        "Actor " + actorEntity.Id + ". No ability in slot " + slot.Value);
                }

                return false;
            }
        }
        else
        {
            var vectorToTarget = actorEntity.Position!.Cell!.Value.DifferenceTo(targetCell);
            var isMelee = vectorToTarget.Length() <= 1;
            abilityEntity = GetDefaultAttack(actorEntity, isMelee, manager);
            if (abilityEntity == null)
            {
                if (actorEntity.HasComponent(EntityComponent.Player))
                {
                    manager.LoggingSystem.WriteLog(
                        manager.Game.Services.Language.NoDefaultAttack(isMelee), actorEntity, manager);
                }
                else
                {
                    throw new InvalidOperationException("Actor " + actorEntity.Id + ". No usable default attack.");
                }

                return false;
            }
        }

        return ActivateAbility(abilityEntity, actorEntity, targetCell, targetActor, manager);
    }

    public virtual GameEntity? GetDefaultAttack(GameEntity actorEntity, bool melee, GameManager manager)
        => manager.AbilitySlottingSystem.GetAbility(
            actorEntity,
            melee
                ? AbilitySlottingSystem.DefaultMeleeAttackSlot
                : AbilitySlottingSystem.DefaultRangedAttackSlot);

    protected virtual bool ActivateAbility(
        GameEntity abilityEntity, GameEntity actorEntity, Point targetCell, GameEntity? targetEntity,
        GameManager manager)
    {
        var ability = abilityEntity.Ability!;
        if ((ability.Activation & ActivationType.Slottable) == 0)
        {
            if (actorEntity.HasComponent(EntityComponent.Player))
            {
                manager.LoggingSystem.WriteLog(
                    manager.Game.Services.Language.AbilityCannotBeActivated(ability), actorEntity, manager);
                return false;
            }

            throw new InvalidOperationException("Ability " + abilityEntity.Id + " cannot be activated directly.");
        }

        var activationMessage = ActivateAbilityMessage.Create(manager);
        activationMessage.AbilityEntity = abilityEntity;
        activationMessage.ActivatorEntity = actorEntity;
        activationMessage.TargetEntity = targetEntity;
        activationMessage.TargetCell = targetCell;

        var isPlayer = actorEntity.HasComponent(EntityComponent.Player);
        if (isPlayer)
        {
            var activationError = manager.AbilityActivationSystem.CanActivateAbility(activationMessage);
            if (activationError != null)
            {
                manager.LoggingSystem.WriteLog(activationError, actorEntity, manager);
                manager.Queue.ReturnMessage(activationMessage);
                return false;
            }
        }
        else if (manager.AbilityActivationSystem.CanActivateAbility(activationMessage) != null)
        {
            manager.Queue.ReturnMessage(activationMessage);
            return false;
        }

        manager.Enqueue(activationMessage);
        return true;
    }

    protected bool Move(
        Direction direction, GameEntity actorEntity, GameManager manager,
        bool onlyChangeHeading = false, bool suppressDelay = false)
    {
        // TODO: only attack on move if hostile
        var position = actorEntity.Position!;
        var targetCell = onlyChangeHeading
            ? position.Cell!.Value
            : position.Cell!.Value.Translate(direction.AsVector());
        if (!onlyChangeHeading
            && targetCell != position.Cell)
        {
            var conflictingActor = position.RoomEntity.Room!.Actors.GetValueOrDefault(targetCell);
            if (conflictingActor != null)
            {
                if (position.Heading != direction)
                {
                    var turnMessage = TravelMessage.Create(manager);
                    turnMessage.ActorEntity = actorEntity;
                    turnMessage.TargetHeading = direction;
                    turnMessage.Distance = 0;
                    turnMessage.SuppressDelay = suppressDelay;

                    manager.Enqueue(turnMessage);
                    return true;
                }

                var abilityEntity = manager.AbilitySlottingSystem.GetAbility(
                    actorEntity, AbilitySlottingSystem.DefaultMeleeAttackSlot);
                return abilityEntity != null
                       && ActivateAbility(abilityEntity, actorEntity, targetCell, conflictingActor, manager);
            }

            var (compulsionSource, isFear) = GetForbiddenCompulsion(actorEntity, position.Cell!.Value, targetCell, manager);
            if (compulsionSource != null)
            {
                // A fear/taunt compulsion forbids this step; the actor can't bring itself to make the
                // move and merely turns to face the way instead, spending its action in place.
                if (actorEntity.HasComponent(EntityComponent.Player))
                {
                    var language = manager.Game.Services.Language;
                    manager.LoggingSystem.WriteLog(
                        isFear ? language.TooAfraidToApproach(compulsionSource) : language.CannotFleeSource(compulsionSource),
                        actorEntity, manager);
                }

                onlyChangeHeading = true;
            }
        }

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = actorEntity;
        travelMessage.TargetHeading = direction;
        travelMessage.Distance = onlyChangeHeading ? 0 : 1;
        travelMessage.MoveOffConflicting = actorEntity.HasComponent(EntityComponent.Player);
        travelMessage.SuppressDelay = suppressDelay;

        manager.Enqueue(travelMessage);
        return true;
    }

    protected static void AddRandomMove(GameEntity actorEntity, GameManager manager)
    {
        var randomMovePercent = actorEntity.Being?.RandomMoveAfterActionPercent ?? 0;
        if (randomMovePercent == 0
            || randomMovePercent < 100
            && manager.Game.Random.Next(0, 100) >= randomMovePercent)
        {
            return;
        }

        if (actorEntity.Being is { IsAlive: true }
            && actorEntity.Position?.Cell != null)
        {
            var direction = (Direction)manager.Game.Random.Next(0, 8);
            var travelMessage = TravelMessage.Create(manager);
            travelMessage.ActorEntity = actorEntity;
            travelMessage.TargetHeading = direction;
            travelMessage.Distance = 1;
            travelMessage.SuppressDelay = true;
            manager.Enqueue(travelMessage);
        }
    }

    protected static Direction? GetPanicDirection(
        GameEntity actorEntity, PositionComponent position, GameManager manager)
    {
        if (FindPerceivedStatusSource(actorEntity, EffectType.Frighten, requiredLevel: 3, manager)
            is not { Position.Cell: { } sourceCell })
        {
            return null;
        }

        return GetPanicDirection(position, sourceCell, manager);
    }

    protected static Direction? GetPanicDirection(
        PositionComponent position, Point sourceCell, GameManager manager)
    {
        var currentCell = position.Cell!.Value;
        var currentDistance = currentCell.DistanceTo(sourceCell);
        Span<Direction> directions = stackalloc Direction[8];
        var directionCount = manager.TravelSystem.GetPossibleMovementDirections(
            position, safe: true, directions);
        Span<Direction> escaping = stackalloc Direction[8];
        Span<Direction> nonApproaching = stackalloc Direction[8];
        var escapingCount = 0;
        var nonApproachingCount = 0;
        for (var i = 0; i < directionCount; i++)
        {
            var direction = directions[i];
            var distance = currentCell.Translate(direction.AsVector()).DistanceTo(sourceCell);
            if (distance > currentDistance)
            {
                escaping[escapingCount++] = direction;
            }
            else if (distance == currentDistance)
            {
                nonApproaching[nonApproachingCount++] = direction;
            }
        }

        var choices = escapingCount != 0
            ? escaping[..escapingCount]
            : nonApproaching[..nonApproachingCount];
        return choices.Length == 0
            ? null
            : choices[manager.Game.Random.Next(0, choices.Length)];
    }

    /// <summary>
    ///     Returns the creature whose fear or taunt forbids a step from <paramref name="fromCell" /> to
    ///     <paramref name="toCell" /> (approaching a fear source or fleeing a taunt source), along with
    ///     whether it is fear. Only a source the actor currently perceives can compel it. Returns
    ///     <c>(null, false)</c> when no compulsion forbids the step.
    /// </summary>
    private static (GameEntity? Source, bool IsFear) GetForbiddenCompulsion(
        GameEntity actorEntity, Point fromCell, Point toCell, GameManager manager)
    {
        if (actorEntity.Being == null
            || actorEntity.ActiveEffects is null)
        {
            return (null, false);
        }

        var forbidden = GetForbiddenSource(EffectType.Frighten, isFear: true);
        if (forbidden != null)
        {
            return (forbidden, true);
        }

        forbidden = GetForbiddenSource(EffectType.Taunt, isFear: false);
        return (forbidden, false);

        GameEntity? GetForbiddenSource(EffectType effectType, bool isFear)
        {
            var requiredLevel = isFear ? 2 : 1;
            if (FindPerceivedStatusSource(actorEntity, effectType, requiredLevel, manager)
                is not { Position.Cell: { } sourceCell } source)
            {
                return null;
            }

            var fromDistance = fromCell.DistanceTo(sourceCell);
            var toDistance = toCell.DistanceTo(sourceCell);
            var forbidden = isFear
                ? toDistance < fromDistance
                : EffectApplicationSystem.GetAbilityLevel(actorEntity, AbilityData.Taunted.Name) >= 3
                    ? toDistance >= fromDistance
                    : toDistance > fromDistance;
            if (forbidden)
            {
                return source;
            }

            return null;
        }
    }

    protected static GameEntity? FindPerceivedStatusSource(
        GameEntity actorEntity, EffectType effectType, int requiredLevel, GameManager manager)
    {
        var abilityName = Effect.GetMetadata(effectType).StatusAbilityName;
        if (abilityName == null
            || EffectApplicationSystem.GetAbilityLevel(actorEntity, abilityName) < requiredLevel
            || AbilityActivationSystem.FindStatusSource(actorEntity, effectType, manager)
                is not { Position: { Cell: not null } sourcePosition } source
            || sourcePosition.RoomId != actorEntity.Position!.RoomId
            || manager.SensorySystem.CanSense(actorEntity, source) is null or SenseType.None)
        {
            return null;
        }

        return source;
    }

    protected GameEntity? GetItem(int itemId, GameEntity actorEntity, GameManager manager)
    {
        var itemEntity = manager.FindEntity(itemId);
        if (itemEntity?.Item?.ContainerId != actorEntity.Id)
        {
            if (actorEntity.HasComponent(EntityComponent.Player))
            {
                manager.LoggingSystem.WriteLog(
                    manager.Game.Services.Language.ItemNotInInventory(), actorEntity, manager);
                return null;
            }

            throw new InvalidOperationException("Actor " + actorEntity.Id + " doesn't have item " + itemId);
        }

        return itemEntity;
    }
}
