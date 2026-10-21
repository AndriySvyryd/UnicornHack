namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Aitvaras = new Creature
    {
        Name = "aitvaras",
        SpeciesClass = SpeciesClass.Bird,
        MovementDelay = 100,
        TurningDelay = 100,
        Size = 2,
        Weight = 30,
        PerceptionBonus = -6,
        MightBonus = -6,
        SpeedBonus = -6,
        FocusBonus = -6,
        Armor = 2,
        MagicResistance = 15,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.Claws,
        SlotCapacity = 1,
        Infravisible = true,
        EnglishDescription = "A squat, birdlike creature with a stiff crest, glossy dark feathers and short, clawed legs. Its long tail trails behind it in a narrow, rust-red plume.",
        InitialLevel = 6,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.NonRecruitable | AIBehavior.Wandering,
        Noise = ActorNoiseType.Hiss
    };
}
