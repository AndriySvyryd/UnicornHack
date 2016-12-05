namespace UnicornHack.Effects
{
    public class MagicalDamage : Effect
    {
        public MagicalDamage()
        {
        }

        public MagicalDamage(Game game)
            : base(game)
        {
        }

        public int Damage { get; set; }

        public override Effect Instantiate(Game game)
            => new MagicalDamage(game) {Damage = Damage};
    }
}