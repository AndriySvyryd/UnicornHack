using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item ThrowingKnife = new Item
        {
            Name = "throwing knife",
            Abilities =
                new HashSet<Ability>
                {
                    new Ability {Activation = ActivationType.OnRangedAttack, Action = AbilityAction.Hit}
                },
            Type = ItemType.WeaponProjectile,
            GenerationWeight = "0",
            Material = Material.Steel,
            Weight = 5
        };
    }
}
