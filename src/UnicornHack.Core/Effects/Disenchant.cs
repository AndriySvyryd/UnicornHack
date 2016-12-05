namespace UnicornHack.Effects
{
    public class Disenchant : Effect
    {
        public Disenchant()
        {
        }

        public Disenchant(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new Disenchant(game);
    }
}