namespace UnicornHack.Data.Players;

public static partial class PlayerRaceData
{
    public static readonly PlayerRace Elf = new PlayerRace
    {
        Name = "elf",
        SpeciesClass = SpeciesClass.Humanoid,
        Size = 6,
        PerceptionBonus = 1,
        MightBonus = -1,
        SpeedBonus = 1,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true,
        InvisibilityDetection = true
    };
}
