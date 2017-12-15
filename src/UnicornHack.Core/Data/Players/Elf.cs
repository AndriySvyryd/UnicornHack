using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Players
{
    public static partial class PlayerRaceData
    {
        public static readonly PlayerRaceDefinition Elf = new PlayerRaceDefinition
        {
            Name = "elf",
            Species = Species.Elf,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Name = "innate",
                    Activation = AbilityActivation.Always,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<bool> {PropertyName = "sleep resistance", Value = true},
                        new ChangeProperty<bool> {PropertyName = "invisibility detection", Value = true},
                        new ChangeProperty<bool> {PropertyName = "infravision", Value = true},
                        new ChangeProperty<bool> {PropertyName = "infravisibility", Value = true},
                        new ChangeProperty<bool> {PropertyName = "humanoidness", Value = true},
                        new ChangeProperty<int> {PropertyName = "strength", Value = -1},
                        new ChangeProperty<int> {PropertyName = "agility", Value = 1},
                        new ChangeProperty<int> {PropertyName = "constitution", Value = -1},
                        new ChangeProperty<int> {PropertyName = "quickness", Value = 1},
                        new ChangeProperty<int>
                        {
                            PropertyName = "size",
                            Value = 6,
                            Function = ValueCombinationFunction.MeanRoundUp
                        }
                    }
                }
            }
        };
    }
}