using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly LauncherVariant FireStaff = new LauncherVariant
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
                    Action = AbilityAction.Shoot,
                    Effects = new HashSet<Effect> {new Burn {Damage = 30}}
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 5}},
            Projectile = "fire bolt"
        };
    }
}