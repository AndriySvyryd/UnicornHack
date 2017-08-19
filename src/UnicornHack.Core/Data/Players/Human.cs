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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Innate",
                    Activation = AbilityActivation.Always,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<bool> {PropertyName = "Infravisibility", Value = true},
                        new ChangeProperty<bool> {PropertyName = "Humanoidness", Value = true}
                    }
                }
            }
        };
    }
}