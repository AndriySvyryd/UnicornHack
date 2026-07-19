namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item PotionOfInhumanity = new Item
    {
        Name = "potion of inhumanity",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "Drink",
                Activation = ActivationType.Manual,
                Action = AbilityAction.Drink,
                Delay = "100",
                Effects = new List<Effect> { new DecreaseAbilityLevel { Duration = EffectDuration.Instant, AbilityName = "human race" }, new RemoveItem() }
            }
        },
        Type = ItemType.Potion,
        Material = Material.Glass,
        Weight = 1
    };
}
