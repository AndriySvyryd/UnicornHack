namespace UnicornHack.Models.GameState
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int PlayerId { get; set; }
        public PlayerCharacter Player { get; set; }
    }
}