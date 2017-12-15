using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Players
{
    public static partial class PlayerRaceData
    {
        public static readonly PlayerRaceDefinition Ogre = new PlayerRaceDefinition
        {
            Name = "ogre",
            Species = Species.Ogre,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Name = "innate",
                    Activation = AbilityActivation.Always,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<bool> {PropertyName = "infravisibility", Value = true},
                        new ChangeProperty<bool> {PropertyName = "humanoidness", Value = true},
                        new ChangeProperty<int> {PropertyName = "strength", Value = 2},
                        new ChangeProperty<int> {PropertyName = "agility", Value = -2},
                        new ChangeProperty<int> {PropertyName = "constitution", Value = 2},
                        new ChangeProperty<int> {PropertyName = "quickness", Value = -2},
                        new ChangeProperty<int>
                        {
                            PropertyName = "size",
                            Value = 9,
                            Function = ValueCombinationFunction.MeanRoundUp
                        }
                    }
                }
            }
        };
    }
}