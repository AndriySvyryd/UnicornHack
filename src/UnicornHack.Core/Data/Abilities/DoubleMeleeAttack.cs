using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Abilities
{
    public static partial class AbilityData
    {
        public static readonly WieldingAbility DoubleMeleeAttack = new WieldingAbility
        {
            Name = "double melee attack",
            Type = AbilityType.DefaultAttack,
            WieldingStyle = WieldingStyle.Dual,
            ItemType = ItemType.WeaponMelee,
            Effects = new HashSet<Effect>
            {
                new Activate(), new Activate()
            }
        };
    }
}
