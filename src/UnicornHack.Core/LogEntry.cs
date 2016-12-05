namespace UnicornHack
{
    public class LogEntry
    {
        protected LogEntry()
        {
        }

        public LogEntry(Player player, string message)
        {
            unchecked
            {
                Id = player.NextLogEntryId++;
            }
            Player = player;
            Message = message;
            Turn = player.Game.CurrentTurn;
        }

        public int Id { get; set; }
        public string Message { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int PlayerId { get; private set; }
        public Player Player { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game => Player.Game;
        public int Turn { get; set; }
        public LogEntryImportance Importance { get; set; }
    }
}