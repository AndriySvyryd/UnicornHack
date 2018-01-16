namespace UnicornHack
{
    public class CommandBase
    {
        protected CommandBase()
        {
        }

        protected CommandBase(int id, Player player, PlayerAction action, int? target, int? target2)
        {
            Id = id;
            Player = player;
            Action = action;
            Target = target;
            Target2 = target2;
        }

        public int GameId { get; set; }
        public Game Game => Player.Game;
        public int Id { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int PlayerId { get; private set; }
        public Player Player { get; set; }

        public PlayerAction Action { get; set; }
        public int? Target { get; set; }
        public int? Target2 { get; set; }
    }
}