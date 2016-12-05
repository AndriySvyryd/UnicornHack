namespace UnicornHack.Effects
{
    public class Blind : Effect
    {
        public Blind()
        {
        }

        public Blind(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Blind(game) {Duration = Duration};
    }
}