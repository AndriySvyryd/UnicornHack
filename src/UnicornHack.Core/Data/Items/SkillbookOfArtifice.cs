namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item SkillbookOfArtifice = new Item
    {
        Name = "skillbook of artifice",
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "Consult",
                Activation = ActivationType.WhileToggled,
                Delay = "100",
                Effects = new List<Effect> { new AddAbility { Duration = EffectDuration.Infinite, Name = "artifice", Level = 1 } }
            }
        },
        Type = ItemType.SkillBook,
        Material = Material.Paper,
        Weight = 1
    };
}
