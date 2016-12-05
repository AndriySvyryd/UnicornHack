namespace UnicornHack.Effects
{
    public class AcidDamage : Effect
    {
        public AcidDamage()
        {
        }

        public AcidDamage(Game game)
            : base(game)
        {
        }

        // Corrodes items
        // Removes stoning
        public int Damage { get; set; }

        public override Effect Instantiate(Game game)
            => new AcidDamage(game) {Damage = Damage};
    }
}