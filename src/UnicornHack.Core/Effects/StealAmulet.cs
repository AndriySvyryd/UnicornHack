namespace UnicornHack.Effects
{
    public class StealAmulet : Effect
    {
        public StealAmulet()
        {
        }

        public StealAmulet(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new StealAmulet(game);
    }
}