using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly LauncherVariant Shortbow = new LauncherVariant
        {
            Name = "shortbow",
            Type = ItemType.WeaponRangedBow,
            Material = Material.Wood,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity | EquipmentSlot.GraspBothExtremities,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnRangedAttack,
                    Action = AbilityAction.Shoot,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 5}},
            Projectile = "arrow"
        };
    }
}