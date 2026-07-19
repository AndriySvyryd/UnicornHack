namespace UnicornHack.Data.Players;

public static partial class PlayerRaceData
{
    public static readonly PlayerRace Ogre = new PlayerRace
    {
        Name = "ogre",
        SpeciesClass = SpeciesClass.Humanoid,
        Size = 9,
        PerceptionBonus = -1,
        MightBonus = 2,
        SpeedBonus = -2,
        FocusBonus = -1,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true
    };
}
