namespace UnicornHack
{
    public class PlayerCommand
    {
        public int GameId { get; set; }
        public int Id { get; set; }
        public Player Player { get; set; }

        public string Command { get; set; }
        public string Target { get; set; }
        public string Target2 { get; set; }
    }
}