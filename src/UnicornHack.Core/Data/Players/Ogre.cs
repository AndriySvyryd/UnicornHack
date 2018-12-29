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
                        new ChangeProperty<bool> {PropertyName = "Infravisible", Value = true},
                        new ChangeProperty<TorsoType>
                        {
                            PropertyName = "TorsoType", Value = TorsoType.Humanoid,
                            Function = ValueCombinationFunction.Override
                        },
                        new ChangeProperty<ExtremityType>
                        {
                            PropertyName = "UpperExtremities", Value = ExtremityType.GraspingFingers,
                            Function = ValueCombinationFunction.Override
                        },
                        new ChangeProperty<ExtremityType>
                        {
                            PropertyName = "LowerExtremities", Value = ExtremityType.Fingers,
                            Function = ValueCombinationFunction.Override
                        },
                        new ChangeProperty<int> {PropertyName = "Might", Value = 2},
                        new ChangeProperty<int> {PropertyName = "Speed", Value = -2},
                        new ChangeProperty<int> {PropertyName = "Perception", Value = -1},
                        new ChangeProperty<int> {PropertyName = "Focus", Value = -1},
                        new ChangeProperty<int>
                            {PropertyName = "Size", Value = 9, Function = ValueCombinationFunction.MeanRoundUp}
                    }
                }
            },
            SkillPointRate = 2,
            TraitPointRate = 2,
            MutationPointRate = 2
        };
    }
}
