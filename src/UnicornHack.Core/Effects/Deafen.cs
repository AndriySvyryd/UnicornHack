namespace UnicornHack.Effects
{
    public class Deafen : Effect
    {
        public Deafen()
        {
        }

        public Deafen(Game game)
            : base(game)
        {
        }

        public int Duration { get; set; }

        public override Effect Instantiate(Game game)
            => new Deafen(game) {Duration = Duration};
    }
}