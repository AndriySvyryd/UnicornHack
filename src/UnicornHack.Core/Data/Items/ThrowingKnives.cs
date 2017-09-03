using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Generation;

namespace UnicornHack.Data.Items
{
    public static partial class ItemVariantData
    {
        public static readonly ItemVariant ThrowingKnives = new ItemVariant
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
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Slash,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 3}}
                },
                new AbilityDefinition
                {
                    Activation = AbilityActivation.OnRangedAttack,
                    Action = AbilityAction.Hit,
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = 5}}
                }
            },
            SimpleProperties = new HashSet<string> {"infinite ammo"},
            ValuedProperties = new Dictionary<string, object> {{"weight", 5}}
        };
    }
}