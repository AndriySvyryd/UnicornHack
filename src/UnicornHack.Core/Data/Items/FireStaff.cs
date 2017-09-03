using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant FireStaff = new ItemVariant
        {
            Name = "fire staff",
            Type = ItemType.WeaponMagicStaff,
            Material = Material.Wood,
            EquipableSizes = SizeCategory.Medium | SizeCategory.Large,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnRangedAttack,
                    Action = AbilityAction.Hit,
                    Effects = new HashSet<Effect> {new Burn {Damage = 5}}
                }
            },
            SimpleProperties = new HashSet<string> { "infinite ammo" },
            ValuedProperties = new Dictionary<string, object> {{"weight", 5}}
        };
    }
}