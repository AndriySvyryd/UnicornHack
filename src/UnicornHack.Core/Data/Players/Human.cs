namespace UnicornHack.Data.Players;

public static partial class PlayerRaceData
{
    public static readonly PlayerRace Human = new PlayerRace
    {
        Name = "human",
        SpeciesClass = SpeciesClass.Humanoid,
        Effects = new Dictionary<int, IReadOnlyList<Effect>>
        {
            {
                3,
                new List<Effect>
                {
                    new AddAbility
                    {
                        Ability = new Ability
                        {
                            Name = "Surge of Luck",
                            Activation = ActivationType.WhileToggled,
                            Cooldown = 200,
                            EnergyPointCost = 50,
                            Effects = new List<Effect> { new ChangeProperty<int> { Amount = "25", PropertyName = "HitPointMaximum" } }
                        }
                    }
                }
            }
        },
        Size = 6,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true
    };
}
