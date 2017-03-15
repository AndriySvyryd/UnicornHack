using System.Linq;
using UnicornHack.Generation.Map;

namespace UnicornHack
{
    public class Stairs
    {
        protected Stairs()
        {
        }

        protected Stairs(Level level, byte x, byte y, Branch targetLevelBranch, byte targetLevelDepth)
        {
            Game = level.Game;
            Id = Game.NextStairsId++;
            Level = level;
            LevelName = level.BranchName;
            LevelDepth = level.Depth;
            LevelX = x;
            LevelY = y;

            TargetLevel = targetLevelBranch.Levels.FirstOrDefault(l => l.Depth == targetLevelDepth)
                          ?? new Level(targetLevelBranch, targetLevelDepth);
            TargetLevelName = targetLevelBranch.Name;
            TargetLevelDepth = targetLevelDepth;

            if (TargetLevel.Width != 0)
            {
                var connectingStairs = TargetLevel.Stairs.FirstOrDefault(s =>
                    s.TargetLevelName == LevelName
                    && s.TargetLevelDepth == LevelDepth
                    && s.TargetLevelX == null);
                if (connectingStairs != null)
                {
                    TargetLevelX = connectingStairs.LevelX;
                    TargetLevelY = connectingStairs.LevelY;
                    connectingStairs.TargetLevelX = LevelX;
                    connectingStairs.TargetLevelY = LevelY;
                }
            }
        }

        public int Id { get; private set; }

        public byte LevelX { get; set; }
        public byte LevelY { get; set; }
        public string LevelName { get; set; }
        public byte LevelDepth { get; set; }
        public Level Level { get; set; }

        public byte? TargetLevelX { get; set; }
        public byte? TargetLevelY { get; set; }
        public string TargetLevelName { get; set; }
        public byte TargetLevelDepth { get; set; }
        public Level TargetLevel { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game { get; set; }

        public static Stairs CreateUpStairs(Game game, Level level, byte x, byte y)
            => new Stairs(level, x, y, level.Branch, (byte)(level.Depth - 1));

        public static Stairs CreateDownStairs(Game game, Level level, byte x, byte y)
            => new Stairs(level, x, y, level.Branch, (byte)(level.Depth + 1));
    }
}