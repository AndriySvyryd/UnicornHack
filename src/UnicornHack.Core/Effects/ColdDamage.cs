namespace UnicornHack.Effects
{
    public class ColdDamage : Effect
    {
        public ColdDamage()
        {
        }

        public ColdDamage(Game game)
            : base(game)
        {
        }

        // Freezes items
        // Removes burning, dissolving
        public int Damage { get; set; }

        public override Effect Instantiate(Game game)
            => new ColdDamage(game) {Damage = Damage};
    }
}