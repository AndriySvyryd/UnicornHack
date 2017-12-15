namespace UnicornHack
{
    public class Launcher : Item
    {
        public Launcher()
        {
        }

        public Launcher(Game game) : base(game)
        {
        }

        public Item Projectile { get; set; }

        protected override void Delete()
        {
            base.Delete();
            if (Projectile != null)
            {
                Projectile.RemoveReference();
                Projectile = null;
            }
        }
    }
}