namespace UnicornHack.Effects
{
    public class MeleeAttack : Effect
    {
        public MeleeAttack()
        {
        }

        public MeleeAttack(Game game)
            : base(game)
        {
        }

        public override Effect Instantiate(Game game)
            => new MeleeAttack(game)
            {
                WeaponId = WeaponId,
                Weapon = Weapon
            };

        public int? WeaponId { get; set; }
        public Item Weapon { get; set; }
    }
}