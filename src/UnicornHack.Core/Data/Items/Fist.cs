using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item Fist = new Item
        {
            Name = "fist",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Activation = ActivationType.OnMeleeAttack,
                    Range = 1,
                    Action = AbilityAction.Punch,
                    Delay = "100*weaponScaling",
                    Effects = new HashSet<Effect> {new PhysicalDamage {Damage = "20*weaponScaling"}}
                }
            },
            Type = ItemType.WeaponMeleeHand,
            GenerationWeight = new DefaultWeight {Multiplier = 0F},
            Material = Material.Flesh,
            Weight = 4,
            RequiredMight = 2,
            RequiredSpeed = 2
        };
    }
}
