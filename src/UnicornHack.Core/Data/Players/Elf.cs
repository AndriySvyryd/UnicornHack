namespace UnicornHack.Data.Players;

public static partial class PlayerRaceData
{
    public static readonly PlayerRace Elf = new PlayerRace
    {
        Name = "elf",
        Species = Species.Elf,
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "elf innate",
                Activation = ActivationType.Always,
                Effects = new List<Effect>
                {
                    new ChangeProperty<bool> { PropertyName = "InvisibilityDetection", Value = true },
                    new ChangeProperty<bool> { PropertyName = "Infravision", Value = true },
                    new ChangeProperty<bool> { PropertyName = "Infravisible", Value = true },
                    new ChangeProperty<TorsoType> { PropertyName = "TorsoType", Value = TorsoType.Humanoid, Function = ValueCombinationFunction.Override },
                    new ChangeProperty<ExtremityType> { PropertyName = "UpperExtremities", Value = ExtremityType.GraspingFingers, Function = ValueCombinationFunction.Override },
                    new ChangeProperty<ExtremityType> { PropertyName = "LowerExtremities", Value = ExtremityType.Fingers, Function = ValueCombinationFunction.Override },
                    new ChangeProperty<int> { PropertyName = "Might", Value = -1 },
                    new ChangeProperty<int> { PropertyName = "Perception", Value = 1 },
                    new ChangeProperty<int> { PropertyName = "Speed", Value = 1 },
                    new ChangeProperty<int> { PropertyName = "Size", Value = 6, Function = ValueCombinationFunction.MeanRoundUp }
                }
            }
        },
        SkillPointRate = 2,
        TraitPointRate = 3,
        MutationPointRate = 1
    };
}
