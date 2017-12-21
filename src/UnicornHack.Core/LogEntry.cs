using System.Collections.Generic;

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
                Id = ++player.NextLogEntryId;
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

        public static readonly IComparer<LogEntry> Comparer = LogComparer.Instance;

        private class LogComparer : IComparer<LogEntry>
        {
            public static readonly LogComparer Instance = new LogComparer();

            private LogComparer()
            {
            }

            public int Compare(LogEntry x, LogEntry y)
            {
                var diff = x.Tick - y.Tick;
                if (diff != 0)
                {
                    return diff;
                }

                return x.Id - y.Id;
            }
        }
    }
}