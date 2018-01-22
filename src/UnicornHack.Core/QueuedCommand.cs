namespace UnicornHack
{
    public class QueuedCommand : CommandBase
    {
        protected QueuedCommand()
        {
        }

        public QueuedCommand(Player player, int tick, PlayerAction action, int? target, int? target2)
            : base(++player.NextQueuedCommandId, tick, player, action, target, target2)
        {
        }
    }
}