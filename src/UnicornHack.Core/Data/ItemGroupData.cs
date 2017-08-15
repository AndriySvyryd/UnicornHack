using System.Collections.Generic;
using UnicornHack.Generation;

namespace UnicornHack.Data
{
    public class ItemGroupData
    {
        public static readonly ItemGroup ItemGroups = new ItemGroup
        {
            SubGroups = new List<ItemGroup>
            {
                new ItemGroup {Type = ItemType.Coin, Weight = 40F},
                new ItemGroup
                {
                    Type = ItemType.WeaponMeleeFist,
                    Weight = 15F,
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
                    Weight = 15F,
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
                    Weight = 15F,
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
                    Weight = 15F,
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
                    Weight = 25F,
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
                    Weight = 25F,
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
                    Weight = 20F,
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
                    Weight = 15F,
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
                    Weight = 15F,
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
                    Weight = 15F,
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
                    Weight = 15F,
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
                    Weight = 20F,
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
                    Weight = 25F,
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
                    Weight = 25F,
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
                    Weight = 25F,
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
                    Weight = 25F,
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
                    Weight = 25F,
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
                    Weight = 25F,
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
                    Weight = 10F,
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
                    Weight = 25F,
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
                    Weight = 20F,
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
                    Weight = 15F,
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