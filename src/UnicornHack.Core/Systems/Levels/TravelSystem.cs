﻿using UnicornHack.Systems.Actors;
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
        traveledMessage.InitialLevel = position.LevelEntity;
        traveledMessage.InitialHeading = heading;
        traveledMessage.InitialLevelCell = position.LevelCell;
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

        if (position.LevelCell == message.TargetCell)
        {
            traveledMessage.Successful = true;
            if (delay != 0)
            {
                DelayMessage.Enqueue(message.ActorEntity, delay, manager);
            }

            return traveledMessage;
        }

        var targetCell = message.TargetCell;
        var level = position.LevelEntity.Level!;
        if (!CanMoveTo(targetCell, level))
        {
            return traveledMessage;
        }

        var targetLevelEntity = position.LevelEntity;
        var connectionEntity = level.Connections.GetValueOrDefault(targetCell);
        if (connectionEntity != null)
        {
            var connection = connectionEntity.Connection!;
            if (connection.Direction == null
                || (connection.Direction & ConnectionDirection.Source) != 0)
            {
                targetLevelEntity = manager.Game.LoadLevel(connection.TargetLevelId)!;
                targetCell = connection.TargetLevelCell!.Value;

                if (pretend)
                {
                    traveledMessage.Successful = true;
                    return traveledMessage;
                }
            }
        }

        var conflictingActor = targetLevelEntity.Level!.Actors.GetValueOrDefault(targetCell);
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

        if (position.LevelId != targetLevelEntity.Id)
        {
            position.SetLevelPosition(targetLevelEntity.Id, targetCell);
        }
        else
        {
            position.LevelCell = targetCell;
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

        position.LevelCell = position.LevelCell.Translate(possibleDirectionsToMove[directionIndex].AsVector());

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = entity;
        travelMessage.TargetHeading = position.Heading!.Value;
        travelMessage.TargetCell = position.LevelCell;

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
        LevelComponent level, Point origin, Point target, Direction initialDirection, bool knownOnly = false)
    {
        var nextPoint = target;

        var path = GetShortestPath(level, origin, target, initialDirection, knownOnly);
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
        LevelComponent level, Point start, Point target, Direction initialDirection, bool knownOnly = false)
        => knownOnly
            ? level.PathFinder!.FindPath(start, target, initialDirection, CanMoveToKnown, level)
            : level.PathFinder!.FindPath(start, target, initialDirection, CanMoveTo, level);

    public IReadOnlyList<Direction> GetPossibleMovementDirections(
        PositionComponent currentPosition,
        bool safe)
    {
        var availableDirections = new List<Direction>();
        for (var i = 0; i < 8; i++)
        {
            var level = currentPosition.LevelEntity.Level!;
            if (CanMoveTo(currentPosition.LevelCell, i, level) == null)
            {
                continue;
            }

            var direction = Vector.MovementDirections[i];
            var targetCell = currentPosition.LevelCell.Translate(direction);
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

    public bool CanMoveTo(Point location, LevelComponent level)
        => CanMoveTo(location.X, location.Y, level) != null;

    private int? CanMoveTo(Point location, int directionIndex, LevelComponent level)
    {
        var direction = Vector.MovementDirections[directionIndex];
        var newLocation = location.Translate(direction);

        return CanMoveTo(newLocation.X, newLocation.Y, level);
    }

    private short? CanMoveTo(byte locationX, byte locationY, LevelComponent level)
    {
        if (locationX >= level.Width || locationY >= level.Height)
        {
            return null;
        }

        var index = level.PointToIndex![locationX, locationY];
        return ((MapFeature)level.Terrain[index]).CanMoveTo() ? index : null;
    }

    private short? CanMoveToKnown(byte locationX, byte locationY, LevelComponent level)
    {
        if (locationX >= level.Width || locationY >= level.Height)
        {
            return null;
        }

        var index = level.PointToIndex![locationX, locationY];
        return ((MapFeature)level.KnownTerrain[index]).CanMoveTo() ? index : null;
    }
}
