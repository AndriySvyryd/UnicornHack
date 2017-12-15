using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly LauncherVariant ThrowingKnives = new LauncherVariant
        {
            Name = "throwing knives",
            Type = ItemType.WeaponRangedThrown,
            Material = Material.Steel,
            EquipableSizes = SizeCategory.Small | SizeCategory.Medium,
            EquipableSlots = EquipmentSlot.GraspSingleExtremity,
            Abilities = new HashSet<AbilityDefinition>
            {
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnRangedAttack,
                    Action = AbilityAction.Throw,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 30}}
                }
            },
            ValuedProperties = new Dictionary<string, object> {{"weight", 5}},
            Projectile = "throwing knife"
        };
    }
}