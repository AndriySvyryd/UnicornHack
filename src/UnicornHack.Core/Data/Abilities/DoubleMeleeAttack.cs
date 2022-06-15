using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Abilities;

public static partial class AbilityData
{
    public static readonly WieldingAbility DoubleMeleeAttack = new WieldingAbility
    {
        Name = "double melee attack",
        Type = AbilityType.DefaultAttack,
        Effects = new List<Effect> { new Activate(), new Activate() },
        WieldingStyle = WieldingStyle.Dual,
        ItemType = ItemType.WeaponMelee
    };
}
