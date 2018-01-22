namespace UnicornHack
{
    public class PlayerCommand : CommandBase
    {
        protected PlayerCommand()
        {
        }

        public PlayerCommand(Player player, int tick, PlayerAction action, int? target, int? target2)
            : base(++player.NextCommandId, tick, player, action, target, target2)
        {
        }
    }
}