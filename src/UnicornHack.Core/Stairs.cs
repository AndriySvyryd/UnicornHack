namespace UnicornHack
{
    public class Stairs
    {
        protected Stairs()
        {
        }

        protected Stairs(Game game, string branchName)
        {
            Id = game.NextStairsId++;
            Game = game;
            BranchName = branchName;
        }

        public int Id { get; private set; }
        public string BranchName { get; set; }

        public byte UpLevelX { get; set; }
        public byte UpLevelY { get; set; }
        public int? UpId { get; set; }
        public Level Up { get; set; }

        public byte DownLevelX { get; set; }
        public byte DownLevelY { get; set; }
        public int? DownId { get; set; }
        public Level Down { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game { get; set; }

        public static Stairs CreateUpStairs(Game game, Level level, byte x, byte y)
            => new Stairs(game, level.Name)
            {
                Down = level,
                DownLevelX = x,
                DownLevelY = y
            };

        public static Stairs CreateDownStairs(Game game, Level level, byte x, byte y)
            => new Stairs(game, level.Name)
            {
                Up = level,
                UpLevelX = x,
                UpLevelY = y
            };
    }
}