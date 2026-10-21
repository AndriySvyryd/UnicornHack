namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature NONE = new Creature
    {
        Name = "NONE",
        Weight = 0,
        PerceptionBonus = -9,
        MightBonus = -9,
        SpeedBonus = -9,
        FocusBonus = -9,
        EnglishDescription = "An empty, shapeless outline with no visible features.",
        InitialLevel = 1,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable
    };
}
