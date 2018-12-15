using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Players
{
    public static partial class PlayerRaceData
    {
        public static readonly PlayerRace Ogre = new PlayerRace
        {
            Name = "ogre",
            Species = Species.Ogre,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "ogre innate",
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
                            PropertyName = "Strength",
                            Value = 2
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "Agility",
                            Value = -2
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "Constitution",
                            Value = 2
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "Quickness",
                            Value = -2
                        },
                        new ChangeProperty<int>
                        {
                            PropertyName = "Size",
                            Value = 9,
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
