using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Data
{
    public static class ItemGroupData
    {
        public static readonly ItemGroup ItemGroups = new ItemGroup
        {
            SubGroups = new List<ItemGroup>
            {
                new ItemGroup {Type = ItemType.Coin, Weight = 0F},
                new ItemGroup
                {
                    Type = ItemType.WeaponMeleeFist,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMeleeShort,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMeleeMedium,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMeleeLong,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMagicFocus,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMagicStaff,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponRangedThrown,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponRangedSlingshot,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponRangedBow,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponRangedCrossbow,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponAmmoContainer,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup {Type = ItemType.WeaponProjectile},
                new ItemGroup
                {
                    Type = ItemType.Shield,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.ArmorTorso,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.ArmorHead,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.ArmorFeet,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.ArmorHands,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.AccessoryBack,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.AccessoryNeck,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.Container,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup {Type = ItemType.Potion, Weight = 100F},
                new ItemGroup
                {
                    Type = ItemType.Wand,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.Figurine,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.Trinket,
                    Weight = 10F,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Complexity = ItemComplexity.Intricate, Weight = 20F},
                        new ItemGroup {Complexity = ItemComplexity.Exotic, Weight = 10F},
                        new ItemGroup {Weight = 40F}
                    }
                },
                new ItemGroup {Type = ItemType.SkillBook, Weight = 50F}
            }
        };
    }
}
