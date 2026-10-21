namespace UnicornHack.Data.Items;

public static partial class ItemData
{
    public static readonly Item Cuirass = new Item
    {
        Name = "cuirass",
        BaseName = "mail armor",
        Abilities = new List<Ability>
        {
            new Ability
            {
                Name = "cuirass protection",
                Activation = ActivationType.WhileEquipped,
                Effects = new List<Effect>
                {
                    new ChangeProperty<int> { Amount = "3", PropertyName = "Armor" },
                    new ChangeProperty<int> { Amount = "10", PropertyName = "Deflection" },
                    new ChangeProperty<int> { Amount = "15", PropertyName = "Hindrance" }
                }
            }
        },
        Type = ItemType.ArmorTorso | ItemType.HeavyArmor,
        GenerationWeight = "$branch == 'ruined stronghold' && $depth >= 2 ? 0.5 : 0",
        RequiredMight = 10
    };
}
