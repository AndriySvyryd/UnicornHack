namespace UnicornHack.Data.Players;

public static partial class PlayerRaceData
{
    public static readonly PlayerRace Human = new PlayerRace
    {
        Name = "human",
        Species = Species.Human,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "human innate",
                Activation = ActivationType.Always,
                Effects = new List<Effect>
                {
                    new ChangeProperty<bool> { PropertyName = "Infravisible", Value = true },
                    new ChangeProperty<TorsoType> { PropertyName = "TorsoType", Value = TorsoType.Humanoid, Function = ValueCombinationFunction.Override },
                    new ChangeProperty<ExtremityType> { PropertyName = "UpperExtremities", Value = ExtremityType.GraspingFingers, Function = ValueCombinationFunction.Override },
                    new ChangeProperty<ExtremityType> { PropertyName = "LowerExtremities", Value = ExtremityType.Fingers, Function = ValueCombinationFunction.Override },
                    new ChangeProperty<int> { PropertyName = "Size", Value = 6, Function = ValueCombinationFunction.MeanRoundUp }
                }
            },
            new Ability
            {
                Name = "Surge of Luck ability",
                Activation = ActivationType.WhileAboveLevel,
                ActivationCondition = 3,
                Effects = new List<Effect>
                {
                    new AddAbility
                    {
                        Ability = new Ability
                        {
                            Name = "Surge of Luck",
                            Activation = ActivationType.WhileToggled,
                            Cooldown = 200,
                            EnergyPointCost = 50,
                            Effects = new List<Effect> { new ChangeProperty<int> { PropertyName = "HitPointMaximum", Value = 25 } }
                        }
                    }
                }
            }
        },
        SkillPointRate = 3,
        TraitPointRate = 3
    };
}
