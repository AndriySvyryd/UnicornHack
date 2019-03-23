using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Abilities
{
    public static partial class AbilityData
    {
        public static readonly WieldingAbility TwoHandedRangedAttack = new WieldingAbility
        {
            Name = "two handed ranged attack",
            Type = AbilityType.DefaultAttack,
            Effects = new HashSet<Effect>
            {
                new Activate()
            },
            WieldingStyle = WieldingStyle.TwoHanded,
            ItemType = ItemType.WeaponRanged
        };
    }
}
