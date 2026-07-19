namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item SkillbookOfLightArmor = new Item
    {
        Name = "skillbook of light armor",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Consult",
                Activation = ActivationType.WhileToggled,
                Delay = "100",
                Effects = new List<Effect> { new AddAbility { Name = "light armor" } }
            }
        },
        Type = ItemType.SkillBook,
        Material = Material.Paper,
        Weight = 1
    };
}
