using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Players
{
    public static partial class PlayerRaceData
    {
        public static readonly PlayerRaceDefinition Human = new PlayerRaceDefinition
        {
            Name = "human",
            Species = Species.Human,
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