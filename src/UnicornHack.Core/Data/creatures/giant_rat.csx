new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 7,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Sqeek,
    PreviousStageName = "sewer rat",
    Name = "giant rat",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent,
    MovementRate = 10,
    Size = Size.Small,
    Weight = 150,
    Nutrition = 75,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}
