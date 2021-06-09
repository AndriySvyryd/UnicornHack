using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item SkillbookOfShortRangeWeapons = new Item
        {
            Name = "skillbook of short range weapons",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Consult",
                    Activation = ActivationType.WhileToggled,
                    Delay = "100",
                    Effects = new List<Effect>
                    {
                        new AddAbility
                        {
                            Duration = EffectDuration.Infinite,
                            Name = "short range weapons",
                            Level = 1
                        }
                    }
                }
            },
            Type = ItemType.SkillBook,
            Material = Material.Paper,
            Weight = 1
        };
    }
}
