using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item SkillbookOfRangedMagicWeapons = new Item
        {
            Name = "skillbook of ranged magic weapons",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Consult",
                    Activation = ActivationType.WhileToggled,
                    Delay = 100,
                    Effects = new HashSet<Effect>
                        {new ChangeProperty<int> {PropertyName = "RangedMagicWeapons", Value = 1, Duration = -1}}
                }
            },
            Type = ItemType.SkillBook,
            Material = Material.Paper,
            Weight = 1
        };
    }
}
