namespace UnicornHack.Data;

public static partial class ItemGroupData
{
    public static readonly ItemGroup ItemGroups = new ItemGroup
    {
        SubGroups = new List<ItemGroup>
        {
            new ItemGroup { Type = ItemType.Coin },
            new ItemGroup
            {
                Type = ItemType.WeaponMeleeHand,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.WeaponMeleeShort,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.WeaponMeleeMedium,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.WeaponMeleeLong,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.WeaponRangedClose,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.WeaponRangedShort,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.WeaponRangedLong,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.WeaponRangedMedium,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.WeaponAmmoContainer,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup { Type = ItemType.WeaponProjectile },
            new ItemGroup
            {
                Type = ItemType.Shield,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.ArmorTorso,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.ArmorHead,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.ArmorFeet,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.ArmorHands,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.AccessoryBack,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.AccessoryNeck,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.Container,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup { Type = ItemType.Potion, Weight = 10F },
            new ItemGroup
            {
                Type = ItemType.Orb,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.Figurine,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup
            {
                Type = ItemType.Trinket,
                Weight = 10F,
                SubGroups = new List<ItemGroup>
                {
                    new ItemGroup { Complexity = ItemComplexity.Intricate, Weight = 20F },
                    new ItemGroup { Complexity = ItemComplexity.Exotic, Weight = 10F },
                    new ItemGroup { Weight = 40F }
                }
            },
            new ItemGroup { Type = ItemType.SkillBook, Weight = 10F }
        }
    };
}
