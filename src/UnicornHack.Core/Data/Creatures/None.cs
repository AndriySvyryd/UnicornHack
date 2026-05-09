namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature NONE = new Creature
    {
        Name = "NONE",
        InitialLevel = 1,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Weight = 0,
        Perception = -9,
        Might = -9,
        Speed = -9,
        Focus = -9
    };
}
