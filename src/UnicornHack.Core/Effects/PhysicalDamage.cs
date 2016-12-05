namespace UnicornHack.Effects
{
    public class PhysicalDamage : Effect
    {
        public PhysicalDamage()
        {
        }

        public PhysicalDamage(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new PhysicalDamage(game) {Damage = Damage};

        public int Damage { get; set; }
    }
}