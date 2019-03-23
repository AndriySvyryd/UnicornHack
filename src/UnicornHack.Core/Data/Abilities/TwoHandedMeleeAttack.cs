using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Abilities
{
    public static partial class AbilityData
    {
        public static readonly WieldingAbility TwoHandedMeleeAttack = new WieldingAbility
        {
            Name = "two handed melee attack",
            Type = AbilityType.DefaultAttack,
            Effects = new HashSet<Effect>
            {
                new Activate()
            },
            WieldingStyle = WieldingStyle.TwoHanded,
            ItemType = ItemType.WeaponMelee
        };
    }
}
