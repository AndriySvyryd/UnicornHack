new Creature
{
    Name = "lizard",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    ArmorClass = 6,
    MovementRate = 6,
    Weight = 50,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    InitialLevel = 4,
    GenerationFrequency = Frequency.Sometimes
}
