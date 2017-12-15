using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant FireBolt = new ItemVariant
        {
            Name = "fire bolt",
            Type = ItemType.WeaponProjectile,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            Material = Material.Plasma,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition {Activation = AbilityActivation.OnRangedAttack, Action = AbilityAction.Hit}
            }
        };
    }
}