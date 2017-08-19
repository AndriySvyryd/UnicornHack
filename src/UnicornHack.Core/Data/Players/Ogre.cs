using System.Collections.Generic;
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
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = AbilityActivation.Always,
                    Effects = new HashSet<Effect>
                    {
                        new ChangeProperty<bool> {PropertyName = "Infravisibility", Value = true},
                        new ChangeProperty<bool> {PropertyName = "Humanoidness", Value = true},
                        new ChangeProperty<int> {PropertyName = "Strength", Value = 2},
                        new ChangeProperty<int> {PropertyName = "Agility", Value = -2},
                        new ChangeProperty<int> {PropertyName = "Constitution", Value = 2},
                        new ChangeProperty<int> {PropertyName = "Quickness", Value = -2},
                        new ChangeProperty<Size> {PropertyName = "Size", Value = Size.Large, IsAbsolute = true}
                    }
                }
            }
        };
    }
}