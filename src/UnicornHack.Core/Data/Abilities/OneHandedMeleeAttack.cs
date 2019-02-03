using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Abilities
{
    public static partial class AbilityData
    {
        public static readonly WieldingAbility OneHandedMeleeAttack = new WieldingAbility
        {
            Name = "one handed melee attack",
            Type = AbilityType.DefaultAttack,
            WieldingStyle = WieldingStyle.OneHanded,
            ItemType = ItemType.WeaponMelee,
            Effects = new HashSet<Effect>
            {
                new Activate()
            }
        };
    }
}
