namespace UnicornHack.Effects
{
    public class PoisonDamage : Effect
    {
        public PoisonDamage()
        {
        }

        public PoisonDamage(Game game)
            : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Instantiate(Game game)
            => new PoisonDamage(game) {Damage = Damage};
    }
}