using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Players
{
    public static partial class PlayerRaceData
    {
        public static readonly PlayerRace Dwarf = new PlayerRace
        {
            Name = "dwarf",
            Species = Species.Dwarf,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "dwarf innate",
                    Activation = ActivationType.Always,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<bool>
                        {
                            PropertyName = "Infravision",
                            Value = true
                        },
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
                            PropertyName = "Strength",
                            Value = 1
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "Agility",
                            Value = -1
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "Constitution",
                            Value = 1
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "Quickness",
                            Value = -1
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "Size",
                            Value = 5,
                            Function = ValueCombinationFunction.MeanRoundUp
                        }
                    }
                }
            },
            SkillPointRate = 2,
            TraitPointRate = 2,
            MutationPointRate = 2
        };
    }
}
