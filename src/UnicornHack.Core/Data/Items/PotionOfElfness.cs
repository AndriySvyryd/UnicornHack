using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item PotionOfElfness = new Item
        {
            Name = "potion of elfness",
            Type = ItemType.Potion,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Material = Material.Glass,
            Weight = 1,
            StackSize = 20,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.ManualActivation,
                    Effects = new HashSet<Effect> {new ChangeRace {RaceName = "elf", Duration = -1 } }
                }
            }
        };
    }
}
