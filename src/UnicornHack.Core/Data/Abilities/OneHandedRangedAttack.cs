using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Abilities
{
    public static partial class AbilityData
    {
        public static readonly WieldingAbility OneHandedRangedAttack = new WieldingAbility
        {
            Name = "one handed ranged attack",
            Type = AbilityType.DefaultAttack,
            WieldingStyle = WieldingStyle.OneHanded,
            ItemType = ItemType.WeaponRanged,
            Effects = new HashSet<Effect>
            {
                new Activate()
            }
        };
    }
}
