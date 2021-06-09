using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item PotionOfDwarfness = new Item
        {
            Name = "potion of dwarfness",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Drink",
                    Activation = ActivationType.Manual,
                    Action = AbilityAction.Drink,
                    Delay = "100",
                    Effects = new List<Effect>
                    {
                        new ChangeRace {Duration = EffectDuration.Infinite, RaceName = "dwarf"},
                        new RemoveItem()
                    }
                }
            },
            Type = ItemType.Potion,
            GenerationWeight = "2",
            Material = Material.Glass,
            Weight = 1,
            StackSize = 20
        };
    }
}
