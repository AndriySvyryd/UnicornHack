using System.Collections.Generic;
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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Innate",
                    Activation = AbilityActivation.Always,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<bool> {PropertyName = "SleepResistance", Value = true},
                        new ChangeProperty<bool> {PropertyName = "InvisibilityDetection", Value = true},
                        new ChangeProperty<bool> {PropertyName = "Infravision", Value = true},
                        new ChangeProperty<bool> {PropertyName = "Infravisibility", Value = true},
                        new ChangeProperty<bool> {PropertyName = "Humanoidness", Value = true},
                        new ChangeProperty<int> {PropertyName = "Strength", Value = -1},
                        new ChangeProperty<int> {PropertyName = "Agility", Value = 1},
                        new ChangeProperty<int> {PropertyName = "Constitution", Value = -1},
                        new ChangeProperty<int> {PropertyName = "Quickness", Value = 1}
                    }
                }
            }
        };
    }
}