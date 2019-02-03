using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item SkillbookOfFireSourcery = new Item
        {
            Name = "skillbook of fire sourcery",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Consult",
                    Activation = ActivationType.WhileToggled,
                    Delay = 100,
                    Effects = new HashSet<Effect>
                        {new AddAbility {AbilityName = "fire sourcery", Level = 1, Duration = EffectDuration.Infinite}}
                }
            },
            Type = ItemType.SkillBook,
            Material = Material.Paper,
            Weight = 1
        };
    }
}
