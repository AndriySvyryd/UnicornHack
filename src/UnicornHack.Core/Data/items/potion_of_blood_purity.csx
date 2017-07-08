new Item
{
    Name = "potion of blood purity",
    Type = ItemType.Potion,
    Weight = 1,
    GenerationWeight = new DefaultWeight { },
    Material = Material.Glass,
    StackSize = 20,
    Abilities = new HashSet<Ability>
    {
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new ChangeRace { RaceName = "prompt", Remove = true } } }
    }
}
