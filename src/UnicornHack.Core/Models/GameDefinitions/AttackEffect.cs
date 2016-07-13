namespace UnicornHack.Models.GameDefinitions
{
    public enum AttackEffect : byte
    {
        PhysicalDamage,
        MagicalDamage,
        FireDamage, // Burns items
        WaterDamage, // Only does damage to actors with water weakness, rusts, dillutes and blanks items
        ColdDamage, // Freezes items
        ElectricityDamage, // Shocks items
        AcidDamage, // Corrodes items
        VenomDamage, // Decays items
        PoisonDamage,
        Disintegrate, // Withers items
        DrainLife,
        DrainEnergy,
        DrainStrength,
        DrainDexterity,
        DrainConstitution,
        DrainIntelligence,
        DrainWisdom,
        DrainCharisma,
        DrainLuck,
        Infect,
        Stone,
        Slime,
        Sleep,
        Slow,
        Paralyze,
        DamageLeg,
        Stun,
        Confuse,
        Hallucinate,
        Blind,
        Deafen,
        Teleport, // Teleports items
        LevelTeleport, // Teleports items
        Polymorph, // Polymorphs items
        ConferLycanthropy,
        Stick,
        Bind,
        Suffocate,
        Engulf,
        Seduce,
        Heal,
        // Item-only
        Curse,
        Disenchant,
        Disarm,
        StealAmulet,
        StealGold,
        StealItem,
        // Special
        Pestilence,
        Famine,
        Death,
        ArcaneSpell,
        DivineSpell
    }
}