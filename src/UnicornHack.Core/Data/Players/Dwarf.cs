using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Players
{
    public static partial class PlayerRaceData
    {
        public static readonly PlayerRaceDefinition Dwarf = new PlayerRaceDefinition
        {
            Name = "dwarf",
            Species = Species.Dwarf,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Name = "innate",
                    Activation = AbilityActivation.Always,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<bool> {PropertyName = "infravision", Value = true},
                        new ChangeProperty<bool> {PropertyName = "infravisibility", Value = true},
                        new ChangeProperty<bool> {PropertyName = "humanoidness", Value = true},
                        new ChangeProperty<int> {PropertyName = "strength", Value = 1},
                        new ChangeProperty<int> {PropertyName = "agility", Value = -1},
                        new ChangeProperty<int> {PropertyName = "constitution", Value = 1},
                        new ChangeProperty<int> {PropertyName = "quickness", Value = -1},
                        new ChangeProperty<int>
                        {
                            PropertyName = "size",
                            Value = 5,
                            Function = ValueCombinationFunction.MeanRoundUp
                        }
                    }
                }
            }
        };
    }
}