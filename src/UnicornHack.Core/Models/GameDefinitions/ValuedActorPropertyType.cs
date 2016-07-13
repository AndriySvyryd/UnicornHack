namespace UnicornHack.Models.GameDefinitions
{
    public enum ValuedActorPropertyType : byte
    {
        Default = 0,
        // byte
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma,
        Luck,
        MovementRate,
        ArmorClass,
        MagicResistance,
        MaxHP,
        // enum
        LegWound, // Left, Right
        MonsterSpeciesAwareness, // Orc, Elf, Human...
        MonsterClassAwareness, // SpeciesClass: All, Undead...
        Lycanthropy // Species
    }
}