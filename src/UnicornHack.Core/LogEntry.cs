namespace UnicornHack
{
    public class LogEntry
    {
        protected LogEntry()
        {
        }

        public LogEntry(Player player, string message, int tick)
        {
            unchecked
            {
                Id = player.NextLogEntryId++;
            }
            Player = player;
            Message = message;
            Tick = tick;
        }

        public int Id { get; set; }
        public string Message { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int PlayerId { get; private set; }
        public Player Player { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game => Player.Game;
        public int Tick { get; set; }
        public LogEntryImportance Importance { get; set; }
    }
}