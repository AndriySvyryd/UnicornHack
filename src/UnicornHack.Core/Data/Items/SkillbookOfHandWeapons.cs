namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item SkillbookOfHandWeapons = new Item
    {
        Name = "skillbook of hand weapons",
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "Consult",
                Activation = ActivationType.WhileToggled,
                Delay = "100",
                Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.Infinite, Name = "hand weapons", Level = 1 } }
            }
        },
        Type = ItemType.SkillBook,
        Material = Material.Paper,
        Weight = 1
    };
}
