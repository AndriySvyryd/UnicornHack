using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item PotionOfInhumanity = new Item
        {
            Name = "potion of inhumanity",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Drink",
                    Activation = ActivationType.Manual,
                    Action = AbilityAction.Drink,
                    Delay = "100",
                    Effects = new HashSet<Effect>
                    {
                        new ChangeRace {RaceName = "human", Remove = true}, new RemoveItem()
                    }
                }
            },
            Type = ItemType.Potion,
            Material = Material.Glass,
            Weight = 1,
            StackSize = 20
        };
    }
}
