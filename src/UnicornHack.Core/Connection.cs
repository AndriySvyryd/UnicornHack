using System.Linq;
using UnicornHack.Generation;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Connection
    {
        protected Connection()
        {
        }

        protected Connection(Level level, Point p, string targetBranchName, byte targetLevelDepth)
        {
            Game = level.Game;
            Id = Game.NextConnectionId++;
            Level = level;
            BranchName = level.BranchName;
            LevelDepth = level.Depth;
            LevelX = p.X;
            LevelY = p.Y;
            level.Connections.Add(this);

            TargetBranchName = targetBranchName;
            TargetLevelDepth = targetLevelDepth;

            TargetBranch = Game.GetBranch(targetBranchName)
                           ?? BranchDefinition.Get(TargetBranchName).Instantiate(Game);

            TargetLevel = Game.GetLevel(targetBranchName, TargetLevelDepth)
                          ?? new Level(TargetBranch, TargetLevelDepth, Level.GenerationRandom.Seed);
            TargetLevel.IncomingConnections.Add(this);

            var connectingStairs = Level.IncomingConnections.FirstOrDefault(s =>
                s.BranchName == targetBranchName
                && s.LevelDepth == targetLevelDepth
                && s.TargetLevelX == null);
            if (connectingStairs != null)
            {
                TargetLevelX = connectingStairs.LevelX;
                TargetLevelY = connectingStairs.LevelY;
                connectingStairs.TargetLevelX = LevelX;
                connectingStairs.TargetLevelY = LevelY;
            }
        }

        public int Id { get; private set; }

        public byte LevelX { get; set; }
        public byte LevelY { get; set; }
        public string BranchName { get; set; }
        public byte LevelDepth { get; set; }
        public Level Level { get; set; }

        public byte? TargetLevelX { get; set; }
        public byte? TargetLevelY { get; set; }
        public string TargetBranchName { get; set; }
        public byte TargetLevelDepth { get; set; }
        public Level TargetLevel { get; set; }
        public Branch TargetBranch { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }

        public static Connection CreateSourceConnection(Level level, Point p, string targetBranchName, byte depth = 1)
            => new Connection(level, p, targetBranchName, depth);

        public static Connection CreateReceivingConnection(Level level, Point p, Connection incoming)
            => new Connection(level, p, incoming.BranchName, incoming.LevelDepth);
    }
}