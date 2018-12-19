using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Players
{
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
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<bool>
                        {
                            PropertyName = "Infravisible",
                            Value = true
                        },
                        new ChangeProperty<TorsoType>
                        {
                            PropertyName = "TorsoType",
                            Value = TorsoType.Humanoid,
                            Function = ValueCombinationFunction.Override
                        },
                        new ChangeProperty<ExtremityType>
                        {
                            PropertyName = "UpperExtremeties",
                            Value = ExtremityType.GraspingFingers,
                            Function = ValueCombinationFunction.Override
                        },
                        new ChangeProperty<ExtremityType>
                        {
                            PropertyName = "LowerExtremeties",
                            Value = ExtremityType.Fingers,
                            Function = ValueCombinationFunction.Override
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "Size",
                            Value = 6,
                            Function = ValueCombinationFunction.MeanRoundUp
                        }
                    }
                },
                new Ability
                {
                    Name = "Surge of Luck ability",
                    Activation = ActivationType.WhileAboveLevel,
                    ActivationCondition = 3,
                    Effects = new HashSet<Effect>
                    {
                        new AddAbility
                        {
                            Ability = new Ability
                            {
                                Name = "Surge of Luck",
                                Activation = ActivationType.WhileToggled,
                                EnergyPointCost = 50,
                                Effects =  new HashSet<Effect>
                                {
                                    new ChangeProperty<int>
                                    {
                                        PropertyName = "HitPointMaximum",
                                        Value = 25,
                                        Function = ValueCombinationFunction.Sum
                                    }
                                }
                            }
                        }
                    }
                }
            },
            SkillPointRate = 3,
            TraitPointRate = 3
        };
    }
}
