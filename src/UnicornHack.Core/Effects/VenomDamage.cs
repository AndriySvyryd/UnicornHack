namespace UnicornHack.Effects
{
    public class VenomDamage : Effect
    {
        public VenomDamage()
        {
        }

        public VenomDamage(Game game)
            : base(game)
        {
        }

        // Decays items, gives temporary venom damage bonus
        public int Damage { get; set; }

        public override Effect Instantiate(Game game)
            => new VenomDamage(game) {Damage = Damage};
    }
}