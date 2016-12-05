namespace UnicornHack.Effects
{
    public class Cripple : Effect
    {
        public Cripple()
        {
        }

        public Cripple(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Cripple(game) {Duration = Duration};
    }
}