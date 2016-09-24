new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Sometimes,
    Name = "lizard",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 50,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}
