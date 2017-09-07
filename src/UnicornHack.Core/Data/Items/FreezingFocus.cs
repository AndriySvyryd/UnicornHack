using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant FreezingFocus = new ItemVariant
        {
            Name = "freezing focus",
            Type = ItemType.WeaponMagicFocus,
            Material = Material.Bone,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Hit,
                    Effects = new HashSet<Effect> {new Freeze {Damage = 40}}
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 5}}
        };
    }
}