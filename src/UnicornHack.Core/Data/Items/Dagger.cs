using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant Dagger = new ItemVariant
        {
            Name = "dagger",
            Type = ItemType.WeaponMeleeShort,
            Material = Material.Steel,
            EquipableSizes = SizeCategory.Tiny | SizeCategory.Small,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Slash,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 40}}
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 5}}
        };
    }
}