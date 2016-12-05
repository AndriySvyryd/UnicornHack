namespace UnicornHack.Effects
{
    public class ElectricityDamage : Effect
    {
        public ElectricityDamage()
        {
        }

        public ElectricityDamage(Game game)
            : base(game)
        {
        }

        // Shocks items
        // Removes slow
        public int Damage { get; set; }

        public override Effect Instantiate(Game game)
            => new ElectricityDamage(game) {Damage = Damage};
    }
}