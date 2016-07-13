namespace UnicornHack.Models.GameState
{
    public class PlayerCommand
    {
        public int GameId { get; set; }
        public int Id { get; set; }
        public PlayerCharacter Player { get; set; }

        public string Command { get; set; }
        public string Target { get; set; }
    }
}