namespace UnicornHack.Models.GameState
{
    public class LogEntry
    {
        protected LogEntry()
        {
        }

        public LogEntry(PlayerCharacter playerCharacter, string message)
        {
            Id = playerCharacter.NextLogEntryId++;
            Player = playerCharacter;
            Message = message;
            Turn = playerCharacter.Game.CurrentTurn;
        }

        public int Id { get; set; }
        public string Message { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int PlayerId { get; private set; }
        public PlayerCharacter Player { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game => Player.Game;
        public int Turn { get; set; }
        public LogEntryImportance Importance { get; set; }
    }
}