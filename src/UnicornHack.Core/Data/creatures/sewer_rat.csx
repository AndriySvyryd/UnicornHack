new CreatureVariant
{
    Name = "sewer rat",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent,
    NextStageName = "giant rat",
    InitialLevel = 1,
    ArmorClass = 7,
    MovementRate = 12,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 50,
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
    GenerationFrequency = Frequency.Often,
    Noise = ActorNoiseType.Sqeek
}
