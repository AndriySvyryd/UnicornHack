namespace UnicornHack.Effects
{
    public class StealItem : Effect
    {
        public StealItem()
        {
        }

        public StealItem(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new StealItem(game);
    }
}