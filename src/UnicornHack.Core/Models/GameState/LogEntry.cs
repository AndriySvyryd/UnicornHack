using System.ComponentModel.DataAnnotations.Schema;

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
        }

        public int Id { get; set; }
        public string Message { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int PlayerId { get; private set; }
        public PlayerCharacter Player { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game => Player.Game;
    }
}