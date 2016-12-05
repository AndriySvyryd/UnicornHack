namespace UnicornHack.Effects
{
    public class WaterDamage : Effect
    {
        public WaterDamage()
        {
        }

        public WaterDamage(Game game)
            : base(game)
        {
        }

        // Only does damage to actors with water weakness, rusts, dillutes and blanks items
        // Removes burning
        public int Damage { get; set; }

        public override Effect Instantiate(Game game)
            => new WaterDamage(game) {Damage = Damage};
    }
}