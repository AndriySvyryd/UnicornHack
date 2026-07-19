namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item PotionOfDwarfness = new Item
    {
        Name = "potion of dwarfness",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Drink",
                Activation = ActivationType.Manual,
                Action = AbilityAction.Drink,
                Delay = "100",
                Effects = new List<Effect> { new AddAbility { Name = "dwarf race" }, new RemoveItem() }
            }
        },
        Type = ItemType.Potion,
        GenerationWeight = "2",
        Material = Material.Glass
    };
}
