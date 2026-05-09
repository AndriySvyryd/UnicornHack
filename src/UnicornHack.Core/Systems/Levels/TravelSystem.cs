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
        var level = position.RoomEntity.Room!;
        if (!CanMoveTo(targetCell, level))
        {
            return traveledMessage;
        }

        var targetLevelEntity = position.RoomEntity;
        var connectionEntity = level.Connections.GetValueOrDefault(targetCell);
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
                    && targetRoom.Level == level.Level;

                if (!isIntraLevel)
                {
                    if (connection.TargetCell == null)
                    {
                        throw new InvalidOperationException(
                            $"Connection from {position.RoomEntity} at {position.Cell} "
                            + $"to {connection.TargetRoomId} has no TargetCell");
                    }

                    targetLevelEntity = manager.Game.LoadRoom(connection.TargetRoomId)!;
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

        var conflictingActor = targetLevelEntity.Room!.Actors.GetValueOrDefault(targetCell);
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

        if (position.RoomId != targetLevelEntity.Id)
        {
            position.SetRoomPosition(targetLevelEntity.Id, targetCell);

            // Reveal hidden connections from the new room.
            var newRoom = targetLevelEntity.Room!;
            foreach (var conn in newRoom.Connections.Values)
            {
                if (conn.Connection!.Hidden)
                {
                    conn.Connection.Hidden = false;
                }
            }

            newRoom.IsVisited = true;
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
            RemoveComponentMessage.Enqueue(message.BeingEntity, EntityComponent.Position, manager);
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

        position.SetRoomPosition(targetRoomEntity.Id, targetDoorCell);

        // Reveal hidden connections + mark visited.
        var enteredRoom = targetRoomEntity.Room!;
        foreach (var conn in enteredRoom.Connections.Values)
        {
            if (conn.Connection!.Hidden)
            {
                conn.Connection.Hidden = false;
            }
        }

        enteredRoom.IsVisited = true;

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
