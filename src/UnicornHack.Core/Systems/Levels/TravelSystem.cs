using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels;

public class TravelSystem : IGameSystem<TravelMessage>, IGameSystem<DiedMessage>
{
    public bool CanTravel(TravelMessage message, GameManager manager)
    {
        var traveled = TryTravel(message, manager, pretend: true);
        var successful = traveled.Successful;
        message.ActorEntity.Manager.ReturnMessage(traveled);
        return successful;
    }

    public MessageProcessingResult Process(TravelMessage message, GameManager manager)
    {
        var traveled = TryTravel(message, manager);
        EnqueueTraveled(traveled, manager);
        return MessageProcessingResult.ContinueProcessing;
    }

    private static void EnqueueTraveled(TraveledMessage message, GameManager manager)
    {
        if (message.Delay != 0)
        {
            DelayMessage.Enqueue(message.Entity, message.Delay, manager);
        }

        manager.Enqueue(message);
    }

    private TraveledMessage TryTravel(TravelMessage message, GameManager manager, bool pretend = false)
    {
        var position = message.ActorEntity.Position!;
        var heading = position.Heading!.Value;
        Debug.Assert(message.Distance is null or >= 0);
        var nonPlanarDirection = message.Distance != null
                                 && message.TargetHeading is Direction.Up or Direction.Down;
        Point? requestedTargetCell;
        if (nonPlanarDirection)
        {
            requestedTargetCell = position.Cell;
        }
        else if (message.Distance is not { } distance)
        {
            requestedTargetCell = message.TargetCell;
        }
        else if (distance == 0)
        {
            requestedTargetCell = position.Cell;
        }
        else
        {
            var targetVector = message.TargetHeading.AsVector();
            requestedTargetCell = position.Cell!.Value.Translate(
                new Vector(targetVector.X * distance, targetVector.Y * distance));
        }

        Debug.Assert(requestedTargetCell != null);
        var traveledMessage = CreateTraveledMessage(
            message.ActorEntity, position, message.TargetHeading, requestedTargetCell.Value, manager);

        var currentRoom = position.RoomEntity.Room!;
        var targetCell = requestedTargetCell.Value;
        var terrainBlocked = !CanMoveTo(targetCell, currentRoom);
        var canInteract = false;
        if (message.ActorEntity.Player != null)
        {
            canInteract = terrainBlocked
                          && TerrainSystem.TryInteract(
                              message.ActorEntity, targetCell, currentRoom, manager, pretend: true);
            var knownObstacle = terrainBlocked
                                && !canInteract
                                && (!currentRoom.IsValid(targetCell)
                                    || currentRoom.KnownTerrain[
                                            currentRoom.PointToIndex![targetCell.X, targetCell.Y]]
                                        != MapFeature.Unexplored);
            if (nonPlanarDirection || knownObstacle)
            {
                return traveledMessage;
            }
        }

        if (position.MovementDelay == 0)
        {
            return traveledMessage;
        }

        var delay = 0;
        var turnDirection = message.TargetHeading;
        if (!pretend
            && position.Heading != turnDirection)
        {
            var octants = turnDirection.ClosestOctantsTo(heading);

            if (!message.SuppressDelay)
            {
                delay += (position.TurningDelay * octants) / 4;
            }

            position.Heading = turnDirection;
        }

        if (position.Cell == requestedTargetCell.Value)
        {
            traveledMessage.Successful = true;
            traveledMessage.Delay = delay;
            return traveledMessage;
        }

        // A rooted actor may turn, but it can't move to a different cell.
        if (!message.ActorEntity.Being!.CanMove)
        {
            return traveledMessage;
        }

        if (!message.SuppressDelay)
        {
            // TODO: Calculate movement delay based on terrain
            delay += position.MovementDelay;
        }
        traveledMessage.Delay = delay;

        var engulfedVictim = manager.EngulferToVictimRelationship.GetVictim(message.ActorEntity);

        if (terrainBlocked)
        {
            if (canInteract)
            {
                traveledMessage.Delay = 0;
                if (!pretend)
                {
                    TerrainSystem.TryInteract(
                        message.ActorEntity, targetCell, currentRoom, manager);
                }
            }

            return traveledMessage;
        }

        var targetRoomEntity = position.RoomEntity;
        var connectionEntity = currentRoom.Connections.GetValueOrDefault(targetCell);
        if (connectionEntity != null)
        {
            var connection = connectionEntity.Connection!;
            if (connection.Directionality == null
                || (connection.Directionality & Directionality.Source) != 0)
            {
                var targetRoom = manager.FindEntity(connection.TargetRoomId)?.Room;

                // Intra-level connections (rooms within the same GameLevel): the player stops on
                // the door tile and the client shows the map dialog. The actual transition happens
                // via ActorActionType.TravelToRoom. Inter-level connections (stairs to a different
                // branch/depth) auto-transition immediately as before.
                var isIntraLevel = targetRoom != null
                    && targetRoom.Level == currentRoom.Level;

                if (!isIntraLevel)
                {
                    targetRoomEntity = manager.Game.LoadRoom(connection.TargetRoomId)!;
                    if (connection.TargetCell == null)
                    {
                        throw new InvalidOperationException(
                            $"Connection from {position.RoomEntity} at {position.Cell} "
                            + $"to {connection.TargetRoomId} has no TargetCell");
                    }

                    targetCell = connection.TargetCell.Value;

                    if (pretend)
                    {
                        traveledMessage.Successful = true;
                        return traveledMessage;
                    }
                }
                else
                {
                    // Let the player land on the door tile without crossing. The
                    // SteppedOnConnectionMessage raised below lets the client decide
                    // whether to surface a room map dialog or some other affordance.
                }
            }
        }

        if (engulfedVictim != null
            && targetRoomEntity != position.RoomEntity)
        {
            return traveledMessage;
        }

        Point? victimInitialCell = null;
        Point? victimTargetCell = null;
        if (engulfedVictim != null)
        {
            if (engulfedVictim.Position is not { Cell: { } currentVictimCell } victimPosition
                || victimPosition.RoomId != position.RoomId)
            {
                return traveledMessage;
            }

            var movement = traveledMessage.InitialLevelCell.DifferenceTo(targetCell);
            var pairedTarget = currentVictimCell.Translate(movement);
            if (!currentRoom.IsValid(pairedTarget)
                || !CanMoveTo(pairedTarget, currentRoom))
            {
                return traveledMessage;
            }

            var conflictingVictimTarget = currentRoom.Actors.GetValueOrDefault(pairedTarget);
            if (conflictingVictimTarget != null
                && conflictingVictimTarget != message.ActorEntity
                && conflictingVictimTarget != engulfedVictim)
            {
                return traveledMessage;
            }

            victimInitialCell = currentVictimCell;
            victimTargetCell = pairedTarget;
        }

        var conflictingActor = targetRoomEntity.Room!.Actors.GetValueOrDefault(targetCell);
        if (conflictingActor != null
            && conflictingActor != engulfedVictim
            && (!message.MoveOffConflicting
                || !MoveOffCell(conflictingActor, manager)))
        {
            return traveledMessage;
        }

        traveledMessage.Successful = true;
        if (pretend)
        {
            return traveledMessage;
        }

        if (engulfedVictim != null
            && victimInitialCell.HasValue
            && victimTargetCell.HasValue)
        {
            var initialVictimCell = victimInitialCell.Value;
            var victimDestination = victimTargetCell.Value;

            // Move the actor vacating an occupied destination first when the pair crosses cells.
            if (targetCell == initialVictimCell)
            {
                var victimTraveled = MoveActor(
                    engulfedVictim, targetRoomEntity, victimDestination, delay: 0, manager);
                manager.Enqueue(victimTraveled);
                MoveActor(
                    message.ActorEntity, targetRoomEntity, targetCell, delay, manager, traveledMessage);
            }
            else
            {
                MoveActor(
                    message.ActorEntity, targetRoomEntity, targetCell, delay, manager, traveledMessage);
                var victimTraveled = MoveActor(
                    engulfedVictim, targetRoomEntity, victimDestination, delay: 0, manager);
                manager.Enqueue(victimTraveled);
            }
        }
        else
        {
            MoveActor(
                message.ActorEntity, targetRoomEntity, targetCell, delay, manager, traveledMessage);
        }

        return traveledMessage;
    }

    private TraveledMessage MoveActor(
        GameEntity actorEntity, GameEntity targetRoomEntity, Point targetCell, int delay,
        GameManager manager, TraveledMessage? traveledMessage = null)
    {
        var position = actorEntity.Position!;
        traveledMessage ??= CreateTraveledMessage(
            actorEntity, position, position.Heading!.Value, targetCell, manager);
        traveledMessage.Delay = delay;
        var currentRoom = position.RoomEntity.Room!;
        if (position.RoomId != targetRoomEntity.Id)
        {
            EnterRoom(actorEntity, position, currentRoom, targetRoomEntity, targetCell, manager);
        }
        else
        {
            position.Cell = targetCell;
        }

        traveledMessage.Successful = true;

        // Moving keeps bleeding wounds open longer.
        manager.EffectApplicationSystem.ExtendEffectsOnMovement(actorEntity, manager);

        if (actorEntity.HasComponent(EntityComponent.Player)
            && position.RoomEntity.Room!.Connections.GetValueOrDefault(position.Cell!.Value)
                is { } landedConnection)
        {
            var stepMessage = SteppedOnConnectionMessage.Create(manager);
            stepMessage.ActorEntity = actorEntity;
            stepMessage.ConnectionEntity = landedConnection;
            manager.Enqueue(stepMessage);
        }

        return traveledMessage;
    }

    private static TraveledMessage CreateTraveledMessage(
        GameEntity actorEntity, PositionComponent position, Direction targetHeading, Point targetCell,
        GameManager manager)
    {
        var traveledMessage = TraveledMessage.Create(manager);
        traveledMessage.Entity = actorEntity;
        traveledMessage.InitialLevel = position.RoomEntity;
        traveledMessage.InitialHeading = position.Heading!.Value;
        traveledMessage.InitialLevelCell = position.Cell!.Value;
        traveledMessage.TargetHeading = targetHeading;
        traveledMessage.TargetCell = targetCell;
        return traveledMessage;
    }

    public MessageProcessingResult Process(DiedMessage message, GameManager manager)
    {
        if (!message.BeingEntity.HasComponent(EntityComponent.Player))
        {
            DeleteEntityMessage.Enqueue(message.BeingEntity, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    /// <summary>
    /// Moves an actor from its current room to an adjacent room via the connection that
    /// targets <paramref name="targetRoomId"/>. The actor lands on the connection's target
    /// cell. Returns false if no such connection exists or the room contains hostile actors
    /// and the actor isn't standing on the source connection tile.
    /// </summary>
    public bool TravelToRoom(GameEntity actorEntity, int targetRoomId, GameManager manager)
    {
        if (EffectApplicationSystem.GetAbilityLevel(actorEntity, AbilityData.Engulfed.Name) > 0
            || manager.EngulferToVictimRelationship.GetVictim(actorEntity) != null)
        {
            return false;
        }

        var position = actorEntity.Position!;
        var currentRoom = position.RoomEntity.Room!;

        // Find the connection from the current room that leads to the target room.
        var connectionToTarget = currentRoom.Connections.Values
            .Select(e => e.Connection!)
            .FirstOrDefault(c => c.TargetRoomId == targetRoomId);

        if (connectionToTarget == null)
        {
            // TODO: Display a message that the actor can't travel to the target room from the current room.
            return false;
        }

        // Travel is allowed if the room has no hostile actors, or the actor is
        // standing on the specific connection tile that leads to the target room.
        // TODO: Check hostility
        var hasHostiles = currentRoom.Actors.Values
            .Any(a => a.HasComponent(EntityComponent.AI));
        if (hasHostiles)
        {
            var connectionTile = connectionToTarget.Entity.Position!.Cell;
            if (position.Cell != connectionTile)
            {
                manager.LoggingSystem.WriteLog(
                    "Clear the room first or move to the door.", actorEntity, manager);
                return false;
            }
        }

        var targetRoomEntity = manager.Game.LoadRoom(targetRoomId)!;
        var targetDoorCell = connectionToTarget.TargetCell!.Value;

        var initialRoomEntity = position.RoomEntity;
        var initialCell = position.Cell!.Value;
        var initialHeading = position.Heading!.Value;

        EnterRoom(actorEntity, position, currentRoom, targetRoomEntity, targetDoorCell, manager);

        // Notify downstream systems (SensorySystem, KnowledgeSystem, ItemMovingSystem)
        // so visibility, the actor's own knowledge entity and pickup-on-move-over all
        // run for the new position.
        var traveledMessage = TraveledMessage.Create(manager);
        traveledMessage.Entity = actorEntity;
        traveledMessage.InitialLevel = initialRoomEntity;
        traveledMessage.InitialHeading = initialHeading;
        traveledMessage.InitialLevelCell = initialCell;
        traveledMessage.TargetHeading = position.Heading!.Value;
        traveledMessage.TargetCell = targetDoorCell;
        traveledMessage.Successful = true;
        traveledMessage.Delay = position.MovementDelay;
        EnqueueTraveled(traveledMessage, manager);
        return true;
    }

    // When an actor moves to a new room, its inventory items (and nested contents) keep their Position but
    // must have their RoomId realigned to the actor's new room.
    private static void PropagateRoomId(GameEntity holder, int roomId, GameManager manager)
    {
        var items = manager.EntityItemsToContainerRelationship.GetDependents(holder);
        if (items != null)
        {
            foreach (var item in items)
            {
                if (item.Position is { } itemPosition
                    && itemPosition.RoomId != roomId)
                {
                    itemPosition.RoomId = roomId;
                }

                PropagateRoomId(item, roomId, manager);
            }
        }

        if (holder.Being is { } being)
        {
            UpdateNaturalWeaponRoom(being.PrimaryNaturalWeaponId, roomId, manager);
            UpdateNaturalWeaponRoom(being.SecondaryNaturalWeaponId, roomId, manager);

        }
    }

    private static void UpdateNaturalWeaponRoom(int? weaponId, int roomId, GameManager manager)
    {
        if (weaponId is { } id
            && manager.FindEntity(id)?.Position is { } weaponPosition
            && weaponPosition.RoomId != roomId)
        {
            weaponPosition.RoomId = roomId;
        }
    }

    /// <summary>
    ///     Moves <paramref name="position" /> into <paramref name="targetRoomEntity" /> at
    ///     <paramref name="targetCell" />, reveals the entered room's hidden connections and
    ///     marks both the room being left (<paramref name="currentRoom" />) and the room being
    ///     entered as visited. Shared by the step-onto-connection (<see cref="TryTravel" />) and
    ///     explicit room-travel (<see cref="TravelToRoom" />) paths.
    ///     <para>
    ///         When the traveling actor is a player and no player remains behind, the abandoned
    ///         room (intra-level move), level (inter-level move within the same branch) or whole
    ///         branch (inter-branch move) is permanently deleted, since a visited room can never be
    ///         revisited. See <see cref="DeleteRoomContents" />, <see cref="DeleteLevelContents" />
    ///         and <see cref="DeleteBranchContents" />.
    ///     </para>
    /// </summary>
    private void EnterRoom(
        GameEntity actorEntity, PositionComponent position, RoomComponent currentRoom,
        GameEntity targetRoomEntity, Point targetCell, GameManager manager)
    {
        var newRoom = targetRoomEntity.Room!;
        var isIntraLevel = newRoom.Level == currentRoom.Level;

        position.SetRoomPosition(targetRoomEntity.Id, targetCell);

        PropagateRoomId(actorEntity, targetRoomEntity.Id, manager);

        foreach (var conn in newRoom.Connections.Values)
        {
            if (conn.Connection!.Hidden)
            {
                conn.Connection.Hidden = false;
            }
        }

        currentRoom.IsVisited = true;
        newRoom.IsVisited = true;

        // Clean up the abandoned space once the player has left it and no other player remains
        // behind, since a visited room/level/branch can never be revisited:
        //   * same level        -> just clear the room the player left (the room stays in the graph);
        //   * different level,
        //     same branch        -> delete the whole abandoned level, keeping the rooms that still
        //                           lead to other reachable levels;
        //   * different branch   -> delete the entire previous branch, every level and all.
        if (actorEntity.HasComponent(EntityComponent.Player)
            && !manager.PlayerSystem.IsPlayerInRoom(currentRoom.Entity.Id, manager))
        {
            var currentLevel = currentRoom.Level;
            if (isIntraLevel)
            {
                DeleteRoomContents(currentRoom, manager);
            }
            else if (newRoom.Level.BranchName == currentLevel.BranchName)
            {
                DeleteLevelContents(currentLevel, manager);
            }
            else
            {
                DeleteBranchContents(currentLevel.Branch, manager);
            }
        }
    }

    /// <summary>
    ///     Deletes every non-player actor and every loose floor item in <paramref name="room" />.
    ///     Called when the player abandons a room they can never return to, so its contents never
    ///     need processing again. The player (and any other player sharing the room) is left
    ///     untouched.
    ///     <para>
    ///         Deletion is deferred via <see cref="DeleteEntityMessage" /> (the same low-priority
    ///         path as death) so the heavy teardown runs after the current cascade settles rather than
    ///         re-entrantly mid-travel. Each queued message keeps its actor alive until processed, so a
    ///         deferred actor survives any intervening room teardown and is then fully removed.
    ///     </para>
    /// </summary>
    public void DeleteRoomContents(RoomComponent room, GameManager manager)
    {
        // Snapshot first: deleting mutates the room's Actors / Items dictionaries.
        foreach (var actorEntity in room.Actors.Values)
        {
            if (!actorEntity.HasComponent(EntityComponent.Player))
            {
                DeleteEntityMessage.Enqueue(actorEntity, manager);
            }
        }

        foreach (var itemEntity in room.Items.Values)
        {
            DeleteEntityMessage.Enqueue(itemEntity, manager);
        }
    }

    /// <summary>
    ///     Permanently deletes every room (and its contents) of a level the player has just left for
    ///     a different level in the same branch. Since a visited level can never be revisited, all of
    ///     it — including the room with the inter-level connection the player used — is freed so it
    ///     never needs processing or persistence again.
    /// </summary>
    public void DeleteLevelContents(GameLevel level, GameManager manager)
    {
        foreach (var room in level.Rooms.ToList())
        {
            DeleteRoom(room, level, manager);
        }

        manager.Game.Repository.DeleteLevel(level);
        level.Branch.Levels.Remove(level);
    }

    /// <summary>
    ///     Permanently deletes every level in <paramref name="branch" />, rooms and all. Called when
    ///     the player leaves a branch for a different one: a branch is traversed one-directionally,
    ///     so once left none of its levels can ever be reached again and nothing in it needs further
    ///     processing or persistence.
    /// </summary>
    public void DeleteBranchContents(GameBranch branch, GameManager manager)
    {
        foreach (var level in branch.Levels.ToList())
        {
            DeleteLevelContents(level, manager);
        }
    }

    /// <summary>
    ///     Deletes <paramref name="room" />'s contents (deferred) and tears the room entity down.
    ///     The containing level is deleted after all loaded rooms are removed.
    /// </summary>
    private void DeleteRoom(RoomComponent room, GameLevel level, GameManager manager)
    {
        DeleteRoomContents(room, manager);

        // Remove the Room component first so EF's NavigationFixer doesn't try to sever the required
        // RoomComponent -> GameLevel relationship by nulling its non-nullable foreign key.
        room.Entity.RemoveComponent(EntityComponent.Room);
        level.Rooms.Remove(room);
    }

    private bool MoveOffCell(GameEntity entity, GameManager manager, bool pretend = false)
    {
        var position = entity.Position!;
        Span<Direction> possibleDirectionsToMove = stackalloc Direction[8];
        var directionCount = GetPossibleMovementDirections(
            position, safe: true, possibleDirectionsToMove);
        if (directionCount == 0)
        {
            // TODO: cascade move actors
            return false;
        }

        var directionIndex = manager.Game.Random.Next(minValue: 0, maxValue: directionCount);

        position.Cell = position.Cell!.Value.Translate(possibleDirectionsToMove[directionIndex].AsVector());

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = entity;
        travelMessage.TargetHeading = position.Heading!.Value;
        travelMessage.TargetCell = position.Cell!.Value;

        var traveledMessage = TryTravel(travelMessage, manager, pretend);
        var successful = traveledMessage.Successful;
        if (!pretend)
        {
            EnqueueTraveled(traveledMessage, manager);
        }
        else
        {
            manager.ReturnMessage(traveledMessage);
        }

        manager.ReturnMessage(travelMessage);
        return successful;
    }

    public Direction? GetFirstStepFromShortestPath(
        RoomComponent room, Point origin, Point target, Direction initialDirection,
        bool knownOnly = false, bool avoidActors = false)
    {
        var nextPoint = target;

        var path = GetShortestPath(room, origin, target, initialDirection, knownOnly, avoidActors);
        if (path == null)
        {
            return null;
        }

        if (path.Count != 0)
        {
            nextPoint = path[^1];
        }

        Debug.Assert(origin.DistanceTo(nextPoint) <= 1);

        return origin.DifferenceTo(nextPoint).AsDirection();
    }

    public List<Point>? GetShortestPath(
        RoomComponent room, Point start, Point target, Direction initialDirection,
        bool knownOnly = false, bool avoidActors = false)
        => avoidActors
            ? room.PathFinder!.FindPath(
                start, target, initialDirection, CanMoveToAvoidingActors, (room, target, knownOnly))
            : knownOnly
            ? room.PathFinder!.FindPath(start, target, initialDirection, CanMoveToKnown, room)
            : room.PathFinder!.FindPath(start, target, initialDirection, CanMoveTo, room);

    public IReadOnlyList<Direction> GetPossibleMovementDirections(
        PositionComponent currentPosition,
        bool safe)
    {
        Span<Direction> directions = stackalloc Direction[8];
        var count = GetPossibleMovementDirections(currentPosition, safe, directions);
        var availableDirections = new List<Direction>(count);
        for (var i = 0; i < count; i++)
        {
            availableDirections.Add(directions[i]);
        }

        return availableDirections;
    }

    public int GetPossibleMovementDirections(
        PositionComponent currentPosition,
        bool safe,
        Span<Direction> availableDirections)
    {
        if (availableDirections.Length < 8)
        {
            throw new ArgumentException("The destination must contain at least eight elements.", nameof(availableDirections));
        }

        var count = 0;
        var level = currentPosition.RoomEntity.Room!;
        for (var i = 0; i < 8; i++)
        {
            if (CanMoveTo(currentPosition.Cell!.Value, i, level) == null)
            {
                continue;
            }

            var direction = Vector.MovementDirections[i];
            var targetCell = currentPosition.Cell!.Value.Translate(direction);
            var connectionEntity = level.Connections.GetValueOrDefault(targetCell);
            if (connectionEntity != null)
            {
                continue;
            }

            if (safe && level.Actors.ContainsKey(targetCell))
            {
                continue;
            }

            availableDirections[count++] = (Direction)i;
        }

        return count;
    }

    // TODO: Use locomotion type
    // TODO: block if directionIndex > 3 (diagonal) and path is too narrow to squeeze through
    // TODO: Also avoid actors (at least adjacent ones)

    public bool CanMoveTo(Point location, RoomComponent room)
        => CanMoveTo(location.X, location.Y, room) != null;

    /// <summary>
    ///     Whether an actor can occupy <paramref name="cell" /> in <paramref name="room" />: it is a valid
    ///     in-room cell, passable, and not already holding another actor.
    /// </summary>
    public bool IsActorFreeCell(Point cell, RoomComponent room)
        => room.IsValid(cell)
            && CanMoveTo(cell, room)
            && !room.Actors.ContainsKey(cell);

    /// <summary>
    ///     Finds a cell in <paramref name="room" /> satisfying <paramref name="condition" />, skipping any
    ///     <paramref name="excluded" /> cell; returns null if none qualifies. With an <paramref name="origin" />
    ///     it steps outward ring by ring up to <paramref name="maxRadius" /> - distance 0 being the origin
    ///     itself - visiting every cell on each concentric square (Chebyshev) ring in direction order starting
    ///     from <paramref name="direction" /> (or East when none is given), so a displaced entity
    ///     spills over along its heading and otherwise settles on the nearest match. Without an origin it scans
    ///     the whole room.
    /// </summary>
    public Point? FindCell(
        RoomComponent room,
        Func<Point, RoomComponent, bool> condition,
        Point? origin = null,
        int maxRadius = 3,
        Point? excluded = null,
        Direction? direction = null)
    {
        if (origin is { } start)
        {
            // Distance 0 is the origin itself.
            if (start != excluded
                && condition(start, room))
            {
                return start;
            }

            // Step outward through concentric Chebyshev squares so a nearer cell always wins. Each octant
            // contributes exactly distance cells; alternating their local traversal direction keeps the
            // ring continuous, while the requested direction only rotates the first octant visited.
            var firstOctant = (direction ?? Direction.East).AsOctants();
            for (var distance = 1; distance <= maxRadius; distance++)
            {
                for (var octantOffset = 0; octantOffset < 8; octantOffset++)
                {
                    var octant = (firstOctant + octantOffset) & 7;
                    var forwards = (octant & 1) == 0;
                    for (var index = 0; index < distance; index++)
                    {
                        var octantY = forwards ? index : distance - index;
                        var relative = TransformFromOctant(distance, octantY, octant);
                        var x = start.X + relative.X;
                        var y = start.Y + relative.Y;
                        if (x < 0 || x >= room.Width
                            || y < 0 || y >= room.Height)
                        {
                            continue;
                        }

                        var cell = new Point((byte)x, (byte)y);
                        if (cell == excluded)
                        {
                            continue;
                        }

                        if (condition(cell, room))
                        {
                            return cell;
                        }
                    }
                }
            }

            return null;
        }

        if (room.IndexToPoint is { } points)
        {
            foreach (var cell in points)
            {
                if (cell != excluded
                    && condition(cell, room))
                {
                    return cell;
                }
            }
        }

        return null;
    }

    private static Vector TransformFromOctant(int x, int y, int octant)
        => octant switch
        {
            0 => new Vector(x, -y),
            1 => new Vector(y, -x),
            2 => new Vector(-y, -x),
            3 => new Vector(-x, -y),
            4 => new Vector(-x, y),
            5 => new Vector(-y, x),
            6 => new Vector(y, x),
            7 => new Vector(x, y),
            _ => throw new ArgumentOutOfRangeException(nameof(octant))
        };

    private int? CanMoveTo(Point location, int directionIndex, RoomComponent room)
    {
        var direction = Vector.MovementDirections[directionIndex];
        var newLocation = location.Translate(direction);

        return CanMoveTo(newLocation.X, newLocation.Y, room);
    }

    private short? CanMoveTo(byte locationX, byte locationY, RoomComponent room)
    {
        if (locationX >= room.Width || locationY >= room.Height)
        {
            return null;
        }

        var index = room.PointToIndex![locationX, locationY];
        return room.Terrain[index].CanMoveTo() ? index : null;
    }

    private short? CanMoveToKnown(byte locationX, byte locationY, RoomComponent room)
    {
        if (locationX >= room.Width || locationY >= room.Height)
        {
            return null;
        }

        var index = room.PointToIndex![locationX, locationY];
        return room.KnownTerrain[index].CanMoveTo() ? index : null;
    }

    private short? CanMoveToAvoidingActors(
        byte locationX, byte locationY, (RoomComponent Room, Point Target, bool KnownOnly) state)
    {
        var index = state.KnownOnly
            ? CanMoveToKnown(locationX, locationY, state.Room)
            : CanMoveTo(locationX, locationY, state.Room);
        if (index == null)
        {
            return null;
        }

        var cell = state.Room.IndexToPoint![index.Value];
        return cell == state.Target || !state.Room.Actors.ContainsKey(cell)
            ? index
            : null;
    }
}
