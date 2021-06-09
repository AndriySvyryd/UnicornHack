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
            Effects = new List<Effect> {new Activate()},
            WieldingStyle = WieldingStyle.OneHanded,
            ItemType = ItemType.WeaponRanged
        };
    }
}
