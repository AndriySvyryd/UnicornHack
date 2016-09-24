namespace UnicornHack.Models.GameDefinitions.Effects
{
    public class WaterDamage : AbilityEffect
    {
        // Only does damage to actors with water weakness, rusts, dillutes and blanks items
        // Removes burning
        public int Damage { get; set; }
    }
}