using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Beings;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels
{
    public class TravelSystem : IGameSystem<TravelMessage>, IGameSystem<DiedMessage>
    {
        public const string TravelMessageName = "Travel";
        public const string TraveledMessageName = "Traveled";

        public TravelMessage CreateTravelMessage(GameManager manager)
            => manager.Queue.CreateMessage<TravelMessage>(TravelMessageName);

        public bool CanTravel(TravelMessage message, GameManager manager)
        {
            using (var traveled = TryTravel(message, manager, pretend: true))
            {
                return traveled.Successful;
            }
        }

        public MessageProcessingResult Process(TravelMessage message, GameManager manager)
        {
            var traveled = TryTravel(message, manager);
            manager.Enqueue(traveled);
            return MessageProcessingResult.ContinueProcessing;
        }

        private TraveledMessage TryTravel(TravelMessage message, GameManager manager, bool pretend = false)
        {
            var position = message.Entity.Position;
            var heading = position.Heading.Value;
            var traveledMessage = manager.Queue.CreateMessage<TraveledMessage>(TraveledMessageName);
            traveledMessage.Entity = message.Entity;
            traveledMessage.InitialLevel = position.LevelEntity;
            traveledMessage.InitialHeading = position.Heading.Value;
            traveledMessage.InitialLevelCell = position.LevelCell;

            if (position.MovementDelay == 0)
            {
                return traveledMessage;
            }

            var turnDirection = message.TargetHeading;
            if (!pretend
                && position.Heading != turnDirection)
            {
                var octants = turnDirection.ClosestOctantsTo(heading);

                traveledMessage.Delay += (position.MovementDelay * octants) / 4;

                position.Heading = turnDirection;
            }

            if (position.LevelCell == message.TargetCell)
            {
                traveledMessage.Successful = true;
                return traveledMessage;
            }

            var targetCell = message.TargetCell;
            var level = position.LevelEntity.Level;
            if (!CanMoveTo(targetCell, level))
            {
                return traveledMessage;
            }

            var targetLevelId = position.LevelId;
            var connectionEntity = manager.ConnectionsToLevelRelationship[position.LevelId]
                .SingleOrDefault(c => c.Position.LevelCell == targetCell);
            if (connectionEntity != null)
            {
                var connection = connectionEntity.Connection;
                targetLevelId = connection.TargetLevelId;
                var targetLevelEntity = manager.Game.LoadLevel(targetLevelId);
                targetCell = connection.TargetLevelCell.Value;

                if (pretend)
                {
                    traveledMessage.Successful = true;
                    return traveledMessage;
                }

                // TODO: Remove connection to the surface
                if (targetLevelEntity.Level.BranchName == "surface")
                {
                    var being = message.Entity.Being;
                    manager.LivingSystem.ChangeCurrentHP(being, -1 * being.HitPoints);
                    return traveledMessage;
                }
            }

            var conflictingActor = manager.LevelActorToLevelCellIndex[(targetLevelId, targetCell.X, targetCell.Y)];
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

            if (position.LevelId != targetLevelId)
            {
                position.SetLevelPosition(targetLevelId, targetCell);
            }
            else
            {
                position.LevelCell = targetCell;
            }

            // TODO: take terrain into account
            traveledMessage.Delay += position.MovementDelay;

            return traveledMessage;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            if (!message.BeingEntity.HasComponent(EntityComponent.Player))
            {
                var removeComponentMessage = manager.CreateRemoveComponentMessage();
                removeComponentMessage.Entity = message.BeingEntity;
                removeComponentMessage.Component = EntityComponent.Position;

                manager.Enqueue(removeComponentMessage, lowPriority: true);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        private bool MoveOffCell(GameEntity entity, GameManager manager, bool pretend = false)
        {
            var position = entity.Position;
            var possibleDirectionsToMove = GetPossibleMovementDirections(position, safe: true, manager);
            if (possibleDirectionsToMove.Count == 0)
            {
                // TODO: cascade move actors
                return false;
            }

            var directionIndex = manager.Game.Random.Next(minValue: 0, maxValue: possibleDirectionsToMove.Count);

            position.LevelCell = position.LevelCell.Translate(possibleDirectionsToMove[directionIndex].AsVector());

            using (var travelMessage = manager.TravelSystem.CreateTravelMessage(manager))
            {
                travelMessage.Entity = entity;
                travelMessage.TargetHeading = position.Heading.Value;
                travelMessage.TargetCell = position.LevelCell;

                var traveledMessage = TryTravel(travelMessage, manager, pretend);
                if (!pretend)
                {
                    manager.Enqueue(traveledMessage);
                }

                return traveledMessage.Successful;
            }
        }

        public Direction? GetFirstStepFromShortestPath(
            LevelComponent level, Point origin, Point target, Direction initialDirection)
        {
            var nextPoint = target;

            var path = GetShortestPath(level, origin, target, initialDirection);
            if (path == null)
            {
                return null;
            }

            if (path.Count != 0)
            {
                nextPoint = path[path.Count - 1];
            }

            Debug.Assert(origin.DistanceTo(nextPoint) <= 1);

            return origin.DifferenceTo(nextPoint).AsDirection();
        }

        public List<Point> GetShortestPath(
            LevelComponent level, Point start, Point target, Direction initialDirection)
            => level.PathFinder.FindPath(start, target, initialDirection, CanMoveTo, level);

        public IReadOnlyList<Direction> GetPossibleMovementDirections(
            PositionComponent currentPosition,
            bool safe,
            GameManager manager)
        {
            var availableDirections = new List<Direction>();
            for (var i = 0; i < 8; i++)
            {
                if (CanMoveTo(currentPosition.LevelCell, i, currentPosition.LevelEntity.Level) == null)
                {
                    continue;
                }

                var direction = Vector.MovementDirections[i];
                var targetCell = currentPosition.LevelCell.Translate(direction);

                var connectionEntity = manager.ConnectionsToLevelRelationship[currentPosition.LevelId]
                    .SingleOrDefault(c => c.Position.LevelCell == targetCell);
                if (connectionEntity != null)
                {
                    continue;
                }

                if (safe
                    && manager.LevelActorToLevelCellIndex[(currentPosition.LevelId, targetCell.X, targetCell.Y)] !=
                    null)
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
        private static bool CanMoveTo(Point location, LevelComponent level)
            => CanMoveTo(location.X, location.Y, level) != null;

        private static int? CanMoveTo(Point location, int directionIndex, LevelComponent level)
        {
            var direction = Vector.MovementDirections[directionIndex];
            var newLocation = location.Translate(direction);

            return CanMoveTo(newLocation.X, newLocation.Y, level);
        }

        private static int? CanMoveTo(byte locationX, byte locationY, LevelComponent level)
        {
            if (locationX >= level.Width || locationY >= level.Height)
            {
                return null;
            }

            var index = level.PointToIndex[locationX, locationY];
            return ((MapFeature)level.Terrain[index]).CanMoveTo() ? (int?)index : null;
        }
    }
}
