namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item SkillbookOfLongWeapons = new Item
    {
        Name = "skillbook of long weapons",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Consult",
                Activation = ActivationType.WhileToggled,
                Delay = "100",
                Effects = new List<Effect> { new AddAbility { Name = "long weapons" } }
            }
        },
        Type = ItemType.SkillBook,
        Material = Material.Paper,
        Weight = 1
    };
}
