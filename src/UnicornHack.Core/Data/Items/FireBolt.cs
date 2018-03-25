using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item FireBolt = new Item
        {
            Name = "fire bolt",
            Type = ItemType.WeaponProjectile,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            Material = Material.Plasma,
            Weight = 0,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnPhysicalRangedAttack,
                    Action = AbilityAction.Hit
                }
            }
        };
    }
}
