using System.Collections.Generic;
using UnicornHack.Utils;

namespace UnicornHack.Systems.Knowledge
{
    public class LogEntry : NotificationEntity
    {
        public static readonly IComparer<LogEntry> Comparer = LogComparer.Instance;

        private int _gameId;
        private int _id;
        private int _playerId;
        private int _tick;
        private string _message;
        private LogEntryImportance _importance;

        public int GameId
        {
            get => _gameId;
            set => SetWithNotify(value, ref _gameId);
        }

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int PlayerId
        {
            get => _playerId;
            set => SetWithNotify(value, ref _playerId);
        }

        public int Tick
        {
            get => _tick;
            set => SetWithNotify(value, ref _tick);
        }

        public string Message
        {
            get => _message;
            set => SetWithNotify(value, ref _message);
        }

        public LogEntryImportance Importance
        {
            get => _importance;
            set => SetWithNotify(value, ref _importance);
        }

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
