namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature BoggartShaman = new Creature
    {
        Name = "boggart shaman",
        SpeciesClass = SpeciesClass.Humanoid,
        MovementDelay = 33,
        TurningDelay = 33,
        Weight = 1000,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        EnglishDescription = "A stooped, sharp-eyed humanoid wrapped in layered scraps of cloth. Knotted cords and small carved charms hang against its chest, while streaks of pigment mark its wrinkled face.",
        InitialLevel = 3,
        GenerationWeight = "0",
        Behavior = AIBehavior.MagicUser,
        Noise = ActorNoiseType.Grunt
    };
}
