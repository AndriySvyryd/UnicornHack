namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item PotionOfElfness = new Item
    {
        Name = "potion of elfness",
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "Drink",
                Activation = ActivationType.Manual,
                Action = AbilityAction.Drink,
                Delay = "100",
                Effects = new List<Effect> { new ChangeRace { Duration = EffectDuration.Infinite, RaceName = "elf" }, new RemoveItem() }
            }
        },
        Type = ItemType.Potion,
        GenerationWeight = "2",
        Material = Material.Glass,
        Weight = 1
    };
}
