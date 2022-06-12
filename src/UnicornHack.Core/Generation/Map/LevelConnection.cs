using System;
using System.Collections.Generic;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map
{
    public class LevelConnection : ICSScriptSerializable
    {
        public string BranchName { get; set; }
        public byte? Depth { get; set; }
        public char? Glyph { get; set; }
        public ConnectionDirection? Direction { get; set; }

        public static ConnectionComponent CreateSourceConnection(
            LevelComponent level, Point cell, LevelConnection connectionDefinition)
        {
            var targetBranchName = connectionDefinition?.BranchName ?? level.BranchName;
            var targetDepth = connectionDefinition?.Depth
                              ?? (byte)(connectionDefinition?.BranchName == null ? level.Depth + 1 : 1);
            var direction = connectionDefinition?.Direction;
            var game = level.Game;
            var manager = game.Manager;
            var branch = game.GetBranch(targetBranchName) ??
                         Branch.Loader.Find(targetBranchName).Instantiate(game);

            var targetLevel = branch.Levels.FirstOrDefault(l => l.Depth == targetDepth);
            Point? targetCell = null;
            if (targetLevel != null)
            {
                var matchingConnection = targetLevel.IncomingConnections
                    .Select(c => c.Connection)
                    .FirstOrDefault(c => c.TargetLevelId == level.EntityId
                                         && c.TargetLevelX == null
                                         && AreMatchingDirection(c.Direction, direction));
                if (matchingConnection != null)
                {
                    if (matchingConnection.Direction == null)
                    {
                        matchingConnection.Direction = GetInverse(direction ?? ConnectionDirection.Both);
                    }

                    if (direction == null)
                    {
                        direction = GetInverse(matchingConnection.Direction.Value);
                    }
                    matchingConnection.TargetLevelCell = cell;
                    targetCell = matchingConnection.Entity.Position.LevelCell;
                }
                else
                {
                    // TODO: Choose a random point
                }
            }
            else
            {
                targetLevel = LevelGenerator.CreateEmpty(branch, targetDepth, level.GenerationRandom.Seed, manager);
            }

            return CreateConnection(level, cell, targetLevel.EntityId, targetCell, direction);
        }

        public static void CreateReceivingConnection(
            LevelComponent level, Point cell, ConnectionComponent incoming, ConnectionDirection? direction)
        {
            if (incoming.Direction == null)
            {
                incoming.Direction = GetInverse(direction ?? ConnectionDirection.Both);
            }

            if (direction == null)
            {
                direction = GetInverse(incoming.Direction.Value);
            }

            incoming.TargetLevelCell = cell;
            var incomingPosition = incoming.Entity.Position;
            CreateConnection(level, cell, incomingPosition.LevelId, incomingPosition.LevelCell, direction);
        }

        public static bool AreMatchingDirection(ConnectionDirection? first, ConnectionDirection? second)
            => first == null
               || second == null
               || first == GetInverse(second.Value);

        private static ConnectionDirection GetInverse(ConnectionDirection direction)
        {
            var inverse = ConnectionDirection.None;
            if ((direction & ConnectionDirection.Source) != 0)
            {
                inverse |= ConnectionDirection.Destination;
            }

            if ((direction & ConnectionDirection.Destination) != 0)
            {
                inverse |= ConnectionDirection.Source;
            }

            return inverse;
        }

        protected static ConnectionComponent CreateConnection(
            LevelComponent level, Point point, int targetLevelId, Point? targetCell, ConnectionDirection? direction)
        {
            var manager = level.Entity.Manager;
            using (var connectionEntityReference = manager.CreateEntity())
            {
                var connectionEntity = connectionEntityReference.Referenced;

                var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
                position.LevelEntity = level.Entity;
                position.LevelCell = point;

                connectionEntity.Position = position;

                var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
                connection.TargetLevelId = targetLevelId;
                connection.TargetLevelCell = targetCell;
                connection.Direction = direction;

                connectionEntity.Connection = connection;


                return connection;
            }
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<LevelConnection>(
                new Dictionary<string, Func<LevelConnection, object, bool>>
                {
                    {nameof(Direction), (o, v) => v != null}
                });

        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}
