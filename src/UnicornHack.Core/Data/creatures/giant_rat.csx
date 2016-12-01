new CreatureVariant
{
    Name = "giant rat",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent,
    PreviousStageName = "sewer rat",
    InitialLevel = 2,
    ArmorClass = 7,
    MovementRate = 10,
    Weight = 150,
    Size = Size.Small,
    Nutrition = 75,
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
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Sqeek
}
