using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item SkillbookOfLifeSourcery = new Item
        {
            Name = "skillbook of life sourcery",
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
                        new ChangeProperty<int> { PropertyName = "LifeSourcery", Value = 1, Duration = -1 }
                    }
                }
            }
        };
    }
}
