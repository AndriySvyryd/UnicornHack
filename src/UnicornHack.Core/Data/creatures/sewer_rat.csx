new Creature
{
    Name = "sewer rat",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent,
    ArmorClass = 7,
    MovementRate = 12,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    InitialLevel = 1,
    NextStageName = "giant rat",
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Often,
    Noise = ActorNoiseType.Sqeek
}
