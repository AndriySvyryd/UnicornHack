namespace UnicornHack.Data.Players;

public static partial class PlayerRaceData
{
    public static readonly PlayerRace Dwarf = new PlayerRace
    {
        Name = "dwarf",
        SpeciesClass = SpeciesClass.Humanoid,
        Size = 5,
        PerceptionBonus = -1,
        MightBonus = 1,
        SpeedBonus = -1,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
