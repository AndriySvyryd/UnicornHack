namespace UnicornHack.Effects
{
    public class Bind : Effect
    {
        public Bind()
        {
        }

        public Bind(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Bind(game) {Duration = Duration};
    }
}