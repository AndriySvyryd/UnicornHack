namespace UnicornHack.Models.GameState
{
    public class Stairs
    {
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

        public Game Game { get; set; }
    }
}