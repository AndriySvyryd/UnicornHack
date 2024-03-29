namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item FlaskOfHealing = new Item
    {
        Name = "flask of healing",
        Abilities = new HashSet<Ability>
        {
            new Ability
            {
                Name = "Drink",
                Activation = ActivationType.Manual,
                Action = AbilityAction.Drink,
                XPCooldown = 100,
                Delay = "100",
                Effects = new List<Effect> { new Heal { Amount = "50" } }
            }
        },
        Type = ItemType.Potion,
        Material = Material.Glass,
        Weight = 1
    };
}
