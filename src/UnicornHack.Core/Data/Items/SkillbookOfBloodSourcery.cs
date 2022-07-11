namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item SkillbookOfBloodSourcery = new Item
    {
        Name = "skillbook of blood sourcery",
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "Consult",
                Activation = ActivationType.WhileToggled,
                Delay = "100",
                Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.Infinite, Name = "blood sourcery", Level = 1 } }
            }
        },
        Type = ItemType.SkillBook,
        Material = Material.Paper,
        Weight = 1
    };
}
