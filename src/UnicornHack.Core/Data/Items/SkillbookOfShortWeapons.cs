using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item SkillbookOfShortWeapons = new Item
        {
            Name = "skillbook of short weapons",
            Type = ItemType.SkillBook,
            Material = Material.Paper,
            Weight = 1,
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Consult",
                    Activation = ActivationType.WhileToggled,
                    Delay = 100,
                    Effects = new HashSet<Effect> {
                        new ChangeProperty<int> { PropertyName = "ShortWeapons", Value = 1, Duration = -1 }
                    }
                }
            }
        };
    }
}
