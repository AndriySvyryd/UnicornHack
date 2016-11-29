new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 8,
    GenerationFrequency = Frequency.Sometimes,
    Name = "gecko",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 6,
    Size = Size.Tiny,
    Weight = 15,
    Nutrition = 15,
    SimpleProperties = new HashSet<string> { "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}
