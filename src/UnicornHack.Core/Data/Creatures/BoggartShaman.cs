namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature BoggartShaman = new Creature
    {
        Name = "boggart shaman",
        SpeciesClass = SpeciesClass.Humanoid,
        InitialLevel = 3,
        GenerationWeight = "5",
        Behavior = AIBehavior.GoldCollector | AIBehavior.MagicUser,
        Noise = ActorNoiseType.Grunt,
        Weight = 1000,
        MovementDelay = 33,
        TurningDelay = 33,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        MagicResistance = 5,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
