namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item PotionOfOgreness = new Item
    {
        Name = "potion of ogreness",
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "Drink",
                Activation = ActivationType.Manual,
                Action = AbilityAction.Drink,
                Delay = "100",
                Effects = new List<Effect> { new ChangeRace { Duration = EffectDuration.Infinite, RaceName = "ogre" }, new RemoveItem() }
            }
        },
        Type = ItemType.Potion,
        GenerationWeight = "2",
        Material = Material.Glass,
        Weight = 1
    };
}
