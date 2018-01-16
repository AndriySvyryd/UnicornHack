namespace UnicornHack
{
    public class PlayerCommand : CommandBase
    {
        protected PlayerCommand()
        {
        }

        public PlayerCommand(Player player, PlayerAction action, int? target, int? target2)
            : base(++player.NextCommandId, player, action, target, target2)
        {
        }
    }
}