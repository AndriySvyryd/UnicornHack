using System;
using System.Collections.Generic;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map
{
    public class LevelConnection : ICSScriptSerializable
    {
        public string BranchName { get; set; }
        public byte? Depth { get; set; }
        public char? Glyph { get; set; }
        public ConnectionDirection Direction { get; set; } = ConnectionDirection.Both;

        public static ConnectionComponent CreateSourceConnection(
            LevelComponent level, Point cell, string targetBranchName, byte targetDepth = 1)
        {
            var game = level.Game;
            var manager = game.Manager;
            var branch = game.GetBranch(targetBranchName) ??
                         Branch.Loader.Find(targetBranchName).Instantiate(game);

            var targetLevel = branch.Levels.FirstOrDefault(l => l.Depth == targetDepth);
            Point? targetCell = null;
            if (targetLevel != null)
            {
                var matchingConnection = manager.IncomingConnectionsToLevelRelationship[targetLevel.EntityId]
                    .Select(c => c.Connection)
                    .FirstOrDefault(c => c.TargetLevelId == level.EntityId && c.TargetLevelX == null);
                if (matchingConnection != null)
                {
                    matchingConnection.TargetLevelCell = cell;
                    targetCell = matchingConnection.Entity.Position.LevelCell;
                }
            }
            else
            {
                targetLevel = LevelGenerator.CreateEmpty(branch, targetDepth, level.GenerationRandom.Seed, manager);
            }

            return CreateConnection(level, cell, targetLevel.EntityId, targetCell);
        }

        public static ConnectionComponent CreateReceivingConnection(
            LevelComponent level, Point cell, ConnectionComponent incoming)
        {
            incoming.TargetLevelCell = cell;
            var incomingPosition = incoming.Entity.Position;
            return CreateConnection(level, cell, incomingPosition.LevelId, incomingPosition.LevelCell);
        }

        protected static ConnectionComponent CreateConnection(
            LevelComponent level, Point point, int targetLevelId, Point? targetCell)
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

                connectionEntity.Connection = connection;


                return connection;
            }
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<LevelConnection>(
                new Dictionary<string, Func<LevelConnection, object, bool>>
                {
                    {nameof(Direction), (o, v) => (ConnectionDirection)v != ConnectionDirection.Both}
                });

        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}
