using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Primitives;

namespace UnicornHack.Data.Items
{
    public static partial class ItemData
    {
        public static readonly Item SkillbookOfLightArmor = new Item
        {
            Name = "skillbook of light armor",
            Abilities = new HashSet<Ability>
            {
                new Ability
                {
                    Name = "Consult",
                    Activation = ActivationType.WhileToggled,
                    Delay = 100,
                    Effects = new HashSet<Effect>
                        {new AddAbility {AbilityName = "light armor", Level = 1, Duration = EffectDuration.Infinite}}
                }
            },
            Type = ItemType.SkillBook,
            Material = Material.Paper,
            Weight = 1
        };
    }
}
