using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item FireBolt = new Item
    {
        Name = "fire bolt",
        Abilities = new HashSet<Ability> { new Ability { Activation = ActivationType.OnRangedAttack, Action = AbilityAction.Hit } },
        Type = ItemType.WeaponProjectile,
        GenerationWeight = "0",
        Material = Material.Plasma,
        Weight = 0
    };
}
