namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item Arrow = new Item
    {
        Name = "arrow",
        Abilities = new List<Ability> { new Ability { Name = "hit", Activation = ActivationType.OnRangedAttack, Action = AbilityAction.Hit } },
        Type = ItemType.WeaponProjectile,
        GenerationWeight = "0",
        Material = Material.Steel,
        Weight = 1
    };
}
