namespace UnicornHack.Effects
{
    public class FireDamage : Effect
    {
        public FireDamage()
        {
        }

        public FireDamage(Game game)
            : base(game)
        {
        }

        // Burns items
        // Removes slime, wet, frozen
        public int Damage { get; set; }

        public override Effect Instantiate(Game game)
            => new FireDamage(game) {Damage = Damage};
    }
}