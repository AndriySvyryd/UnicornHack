using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item FlaskOfHealing = new Item
        {
            Name = "flask of healing",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Drink",
                    Activation = ActivationType.Manual,
                    Action = AbilityAction.Drink,
                    XPCooldown = 25,
                    Delay = "100",
                    Effects = new HashSet<Effect> {new Heal {Amount = "50"}}
                }
            },
            Type = ItemType.Potion,
            Material = Material.Glass,
            Weight = 1
        };
    }
}
