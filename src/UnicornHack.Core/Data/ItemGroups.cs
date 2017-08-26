using System.Collections.Generic;
using UnicornHack.Generation;

namespace UnicornHack.Data
{
    public static class ItemGroupData
    {
        public static readonly ItemGroup ItemGroups = new ItemGroup
        {
            SubGroups = new List<ItemGroup>
            {
                new ItemGroup {Type = ItemType.Coin, Weight = 40F},
                new ItemGroup
                {
                    Type = ItemType.WeaponMeleeFist,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMeleeShort,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMeleeMedium,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMeleeLong,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMagicFocus,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponMagicStaff,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponRangedThrown,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponRangedSlingshot,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponRangedBow,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponRangedCrossbow,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.WeaponAmmoContainer,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.Shield,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.ArmorBody,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.ArmorHead,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.ArmorFeet,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.ArmorHands,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.ArmorBack,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.Accessory,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.Container,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup {Type = ItemType.Potion, Weight = 100F},
                new ItemGroup
                {
                    Type = ItemType.Wand,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.Figurine,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup
                {
                    Type = ItemType.Trinket,
                    SubGroups = new List<ItemGroup>
                    {
                        new ItemGroup {Weight = 40F},
                        new ItemGroup {Type = ItemType.Intricate, Weight = 20F},
                        new ItemGroup {Type = ItemType.Exotic, Weight = 10F}
                    }
                },
                new ItemGroup {Type = ItemType.SkillBook, Weight = 50F}
            }
        };
    }
}