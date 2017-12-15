using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant ThrowingKnife = new ItemVariant
        {
            Name = "throwing knife",
            Type = ItemType.WeaponProjectile,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            Material = Material.Steel,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition {Activation = AbilityActivation.OnRangedAttack, Action = AbilityAction.Hit}
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 5}}
        };
    }
}