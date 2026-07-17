using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
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
        manager.Enqueue(traveled);
        return MessageProcessingResult.ContinueProcessing;
    }

    private TraveledMessage TryTravel(TravelMessage message, GameManager manager, bool pretend = false)
    {
        var position = message.ActorEntity.Position!;
        var heading = position.Heading!.Value;
        var traveledMessage = TraveledMessage.Create(manager);
        traveledMessage.Entity = message.ActorEntity;
        traveledMessage.InitialLevel = position.RoomEntity;
        traveledMessage.InitialHeading = heading;
        traveledMessage.InitialLevelCell = position.Cell;
        traveledMessage.TargetHeading = message.TargetHeading;
        traveledMessage.TargetCell = message.TargetCell;

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

            delay += (position.TurningDelay * octants) / 4;

            position.Heading = turnDirection;
        }

        if (position.Cell == message.TargetCell)
        {
            traveledMessage.Successful = true;
            if (delay != 0)
            {
                DelayMessage.Enqueue(message.ActorEntity, delay, manager);
            }

            return traveledMessage;
        }

        var targetCell = message.TargetCell;
        var currentRoom = position.RoomEntity.Room!;
        if (!CanMoveTo(targetCell, currentRoom))
        {
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

        var conflictingActor = targetRoomEntity.Room!.Actors.GetValueOrDefault(targetCell);
        if (conflictingActor != null
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

        if (position.RoomId != targetRoomEntity.Id)
        {
            EnterRoom(message.ActorEntity, position, currentRoom, targetRoomEntity, targetCell, manager);
        }
        else
        {
            position.Cell = targetCell;
        }

        if (message.ActorEntity.HasComponent(EntityComponent.Player))
        {
            var landedConnection = position.RoomEntity.Room!.Connections.GetValueOrDefault(position.Cell);
            if (landedConnection != null)
            {
                var stepMessage = SteppedOnConnectionMessage.Create(manager);
                stepMessage.ActorEntity = message.ActorEntity;
                stepMessage.ConnectionEntity = landedConnection;
                manager.Enqueue(stepMessage);
            }
        }

        // TODO: take terrain into account
        delay += position.MovementDelay;
        DelayMessage.Enqueue(message.ActorEntity, delay, manager);

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
        var initialCell = position.Cell;
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
        manager.Enqueue(traveledMessage);

        DelayMessage.Enqueue(actorEntity, position.MovementDelay, manager);
        return true;
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
    }

    /// <summary>
    ///     Permanently deletes every level in <paramref name="branch" />, rooms and all. Called when
    ///     the player leaves a branch for a different one: a branch is traversed one-directionally,
    ///     so once left none of its levels can ever be reached again and nothing in it needs further
    ///     processing or persistence.
    /// </summary>
    public void DeleteBranchContents(GameBranch branch, GameManager manager)
    {
        foreach (var level in branch.Levels)
        {
            DeleteLevelContents(level, manager);
        }

        branch.Levels.Clear();
    }

    /// <summary>
    ///     Deletes <paramref name="room" />'s contents (deferred) and tears the room entity down,
    ///     removing it from <paramref name="level" />.
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
        var possibleDirectionsToMove = GetPossibleMovementDirections(position, safe: true);
        if (possibleDirectionsToMove.Count == 0)
        {
            // TODO: cascade move actors
            return false;
        }

        var directionIndex = manager.Game.Random.Next(minValue: 0, maxValue: possibleDirectionsToMove.Count);

        position.Cell = position.Cell.Translate(possibleDirectionsToMove[directionIndex].AsVector());

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = entity;
        travelMessage.TargetHeading = position.Heading!.Value;
        travelMessage.TargetCell = position.Cell;

        var traveledMessage = TryTravel(travelMessage, manager, pretend);
        var successful = traveledMessage.Successful;
        if (!pretend)
        {
            manager.Enqueue(traveledMessage);
        }
        else
        {
            manager.ReturnMessage(traveledMessage);
        }

        manager.ReturnMessage(travelMessage);
        return successful;
    }

    public Direction? GetFirstStepFromShortestPath(
        RoomComponent room, Point origin, Point target, Direction initialDirection, bool knownOnly = false)
    {
        var nextPoint = target;

        var path = GetShortestPath(room, origin, target, initialDirection, knownOnly);
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
        RoomComponent room, Point start, Point target, Direction initialDirection, bool knownOnly = false)
        => knownOnly
            ? room.PathFinder!.FindPath(start, target, initialDirection, CanMoveToKnown, room)
            : room.PathFinder!.FindPath(start, target, initialDirection, CanMoveTo, room);

    public IReadOnlyList<Direction> GetPossibleMovementDirections(
        PositionComponent currentPosition,
        bool safe)
    {
        var availableDirections = new List<Direction>();
        for (var i = 0; i < 8; i++)
        {
            var level = currentPosition.RoomEntity.Room!;
            if (CanMoveTo(currentPosition.Cell, i, level) == null)
            {
                continue;
            }

            var direction = Vector.MovementDirections[i];
            var targetCell = currentPosition.Cell.Translate(direction);
            var connectionEntity = level.Connections.GetValueOrDefault(targetCell);
            if (connectionEntity != null)
            {
                continue;
            }

            if (safe && level.Actors.ContainsKey(targetCell))
            {
                continue;
            }

            availableDirections.Add((Direction)i);
        }

        return availableDirections;
    }

    // TODO: Use locomotion type
    // TODO: block if directionIndex > 3 (diagonal) and path is too narrow to squeeze through
    // TODO: Also avoid actors (at least adjacent ones)

    public bool CanMoveTo(Point location, RoomComponent room)
        => CanMoveTo(location.X, location.Y, room) != null;

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
}
