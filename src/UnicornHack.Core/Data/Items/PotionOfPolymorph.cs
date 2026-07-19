namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item PotionOfPolymorph = new Item
    {
        Name = "potion of polymorph",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Drink",
                Activation = ActivationType.Manual,
                Action = AbilityAction.Drink,
                Delay = "100",
                Effects = new List<Effect> { new Polymorph { CreatureName = "gnome" }, new RemoveItem() }
            }
        },
        Type = ItemType.Potion,
        GenerationWeight = "1",
        Material = Material.Glass,
        Weight = 1
    };
}
