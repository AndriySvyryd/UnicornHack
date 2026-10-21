namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature Henwen = new Creature
    {
        Name = "Henwen",
        SpeciesClass = SpeciesClass.Quadrupedal,
        Size = 3,
        Weight = 1400,
        MightBonus = 4,
        SpeedBonus = -2,
        Armor = 1,
        TorsoType = TorsoType.Quadruped,
        UpperExtremities = ExtremityType.None,
        LowerExtremities = ExtremityType.None,
        SlotCapacity = 1,
        Infravisible = true,
        EnglishDescription = "A great white sow with a heavy belly, a bristled back and broad, mud-darkened hooves. Her pale ears droop over small eyes, and her blunt snout sweeps close to the ground.",
        InitialLevel = 3,
        GenerationWeight = "0",
        Behavior = AIBehavior.Peaceful | AIBehavior.Roaming,
        Noise = ActorNoiseType.Grunt
    };
}
