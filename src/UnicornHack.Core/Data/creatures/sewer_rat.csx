new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 7,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Often,
    Noise = ActorNoiseType.Sqeek,
    NextStageName = "giant rat",
    Name = "sewer rat",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 100,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
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
