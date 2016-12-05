namespace UnicornHack.Effects
{
    public class Stick : Effect
    {
        public Stick()
        {
        }

        public Stick(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new Stick(game);
    }
}