namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item FireBolt = new Item
    {
        Name = "fire bolt",
        Abilities = new List<Ability> { new Ability { Name = "hit", Activation = ActivationType.OnRangedAttack, Action = AbilityAction.Hit } },
        Type = ItemType.WeaponProjectile,
        GenerationWeight = "0",
        Material = Material.Plasma,
        Weight = 0
    };
}
