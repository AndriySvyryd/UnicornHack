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
                    Activation = ActivationType.ManualActivation,
                    Action = AbilityAction.Drink,
                    Delay = "100",
                    Effects = new HashSet<Effect>
                    {
                        new ChangeRace {RaceName = "dwarf", Duration = EffectDuration.Infinite}, new RemoveItem()
                    }
                }
            },
            Type = ItemType.Potion,
            GenerationWeight = new DefaultWeight {Multiplier = 2F},
            Material = Material.Glass,
            Weight = 1,
            StackSize = 20
        };
    }
}
