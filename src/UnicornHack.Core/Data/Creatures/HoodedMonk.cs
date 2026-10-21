namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature HoodedMonk = new Creature
    {
        Name = "hooded monk",
        SpeciesClass = SpeciesClass.Humanoid,
        Size = 8,
        Weight = 1000,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        SlotCapacity = 0,
        Infravisible = true,
        EnglishDescription = "A straight-backed figure in a plain, ankle-length robe tied with a worn cord. A deep hood conceals most of the face, leaving only the chin and folded hands clearly visible.",
        InitialLevel = 2,
        GenerationWeight = "0",
        GenerationFlags = CreatureGenerationFlags.NonGenocidable | CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.Peaceful | AIBehavior.NonRecruitable | AIBehavior.Wandering,
        Noise = ActorNoiseType.Mumble,
        Sex = Sex.Male
    };
}
