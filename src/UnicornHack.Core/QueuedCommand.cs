namespace UnicornHack
{
    public class QueuedCommand : CommandBase
    {
        protected QueuedCommand()
        {
        }

        public QueuedCommand(Player player, PlayerAction action, int? target, int? target2)
            : base(++player.NextQueuedCommandId, player, action, target, target2)
        {
        }
    }
}