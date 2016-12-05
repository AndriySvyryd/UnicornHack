namespace UnicornHack.Effects
{
    public class Teleport : Effect
    {
        // Teleports items too
        public Teleport()
        {
        }

        public Teleport(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new Teleport(game);
    }
}