using System;
using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Generation
{
    public class ItemGenerator
    {
        public virtual byte ExpectedInitialCount { get; set; } = 6;

        private static readonly IReadOnlyList<(ItemType, float)> TypeWeights = new List<(ItemType, float)>
        {
            (ItemType.Coin, 40 ),
            (ItemType.WeaponMeleeFist, 15 ),
            (ItemType.WeaponMeleeShort, 15 ),
            (ItemType.WeaponMeleeMedium, 15 ),
            (ItemType.WeaponMeleeLong, 15 ),
            (ItemType.WeaponMagicFocus, 25 ),
            (ItemType.WeaponMagicStaff, 25 ),
            (ItemType.WeaponRangedThrown, 20 ),
            (ItemType.WeaponRangedSlingshot, 15 ),
            (ItemType.WeaponRangedBow, 15 ),
            (ItemType.WeaponRangedCrossbow, 15 ),
            (ItemType.WeaponAmmoContainer, 15 ),
            (ItemType.Shield, 20 ),
            (ItemType.ArmorBody, 25 ),
            (ItemType.ArmorHead, 25 ),
            (ItemType.ArmorFeet, 25 ),
            (ItemType.ArmorHands, 25 ),
            (ItemType.ArmorBack, 25 ),
            (ItemType.Accessory, 25 ),
            (ItemType.Container, 10 ),
            (ItemType.Potion, 100 ),
            (ItemType.Wand, 25 ),
            (ItemType.Figurine, 20 ),
            (ItemType.Trinket, 15 ),
            (ItemType.SkillBook, 50 )
        };

        private static readonly IReadOnlyList<ItemType> NoQuality =
            new[] {ItemType.None};

        private static readonly IReadOnlyList<ItemType> Qualities =
            new[] {ItemType.None, ItemType.Intricate, ItemType.Exotic};

        private static readonly IReadOnlyDictionary<ItemType, IReadOnlyList<ItemType>> TypeQualities =
            new Dictionary<ItemType, IReadOnlyList<ItemType>>
            {
                {ItemType.Coin, NoQuality},
                {ItemType.WeaponMeleeFist, Qualities},
                {ItemType.WeaponMeleeShort, Qualities},
                {ItemType.WeaponMeleeMedium, Qualities},
                {ItemType.WeaponMeleeLong, Qualities},
                {ItemType.WeaponMagicFocus, Qualities},
                {ItemType.WeaponMagicStaff, Qualities},
                {ItemType.WeaponRangedThrown, Qualities},
                {ItemType.WeaponRangedSlingshot, Qualities},
                {ItemType.WeaponRangedBow, Qualities},
                {ItemType.WeaponRangedCrossbow, Qualities},
                {ItemType.WeaponAmmoContainer, Qualities},
                {ItemType.Shield, Qualities},
                {ItemType.ArmorBody, Qualities},
                {ItemType.ArmorHead, Qualities},
                {ItemType.ArmorFeet, Qualities},
                {ItemType.ArmorHands, Qualities},
                {ItemType.ArmorBack, Qualities},
                {ItemType.Accessory, Qualities},
                {ItemType.Container, Qualities},
                {ItemType.Potion, NoQuality},
                {ItemType.Wand, Qualities},
                {ItemType.Figurine, Qualities},
                {ItemType.Trinket, Qualities},
                {ItemType.SkillBook, NoQuality}
            };

        public virtual void Fill(Level level)
        {
            var itemsToPlace =
                level.GenerationRandom.NextBinomial(0.5f, (ExpectedInitialCount - level.Items.Count) * 2);
            var roomsToFill = level.Rooms.Count;
            var difficultyFraction = (float)(level.Difficulty - 1) / Level.MaxDifficulty;
            foreach (var room in level.GenerationRandom.WeightedOrder(level.Rooms.ToList(), r => r.InsidePoints.Count))
            {
                var itemsPerRoom = (int)Math.Ceiling((float)itemsToPlace / roomsToFill);
                for (var i = 0; i < itemsPerRoom; i++)
                {
                    if (!level.GenerationRandom.TryPick(room.InsidePoints,
                        p => !level.Items.Any(c => c.LevelX == p.X && c.LevelY == p.Y),
                        out var point))
                    {
                        goto NextRoom;
                    }

                    foreach (var typeTuple in level.GenerationRandom.WeightedOrder(TypeWeights, w => w.Item2))
                    {
                        var itemType = typeTuple.Item1;
                        var qualityList = TypeQualities[itemType];
                        var qualityIndex =
                            level.GenerationRandom.NextBinomial(difficultyFraction, qualityList.Count - 1);
                        var quality = qualityList[qualityIndex];
                        foreach (var itemVariant in level.GenerationRandom.WeightedOrder(
                            Item.Loader.GetAllValues(itemType),
                            c => c.Type.HasFlag(quality) ? c.GetWeight(level) : 0))
                        {
                            var quantity = itemType == ItemType.Coin
                                ? level.GenerationRandom.NextBinomial(difficultyFraction, 49) + 1
                                : 1;
                            if (itemVariant.Instantiate(new LevelCell(level, point.X, point.Y), quantity) != null)
                            {
                                goto ItemPlaced;
                            }
                        }
                        // TODO: Log no item of the chosen type/rarity
                    }

                    ItemPlaced:
                    itemsToPlace--;
                }

                NextRoom:
                roomsToFill--;
            }
        }
    }
}