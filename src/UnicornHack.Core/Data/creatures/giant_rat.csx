new Creature
{
    Name = "giant rat",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent,
    ArmorClass = 7,
    MovementRate = 10,
    Weight = 150,
    Size = Size.Small,
    Nutrition = 75,
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
    InitialLevel = 2,
    PreviousStageName = "sewer rat",
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Sqeek
}
