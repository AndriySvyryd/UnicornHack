namespace UnicornHack.Effects
{
    public class StealGold : Effect
    {
        public StealGold()
        {
        }

        public StealGold(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new StealGold(game);
    }
}