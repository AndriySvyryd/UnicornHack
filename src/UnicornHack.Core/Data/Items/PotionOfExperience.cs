namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item PotionOfExperience = new Item
    {
        Name = "potion of experience",
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "Drink",
                Activation = ActivationType.Manual,
                Action = AbilityAction.Drink,
                Delay = "100",
                Effects = new List<Effect> { new GainXP { Amount = "1000" }, new RemoveItem() }
            }
        },
        Type = ItemType.Potion,
        GenerationWeight = "0",
        Material = Material.Glass,
        Weight = 1,
        StackSize = 20
    };
}
