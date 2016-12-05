namespace UnicornHack.Effects
{
    public class Polymorph : Effect
    {
        // Polymorphs items too
        public Polymorph()
        {
        }

        public Polymorph(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new Polymorph(game);
    }
}